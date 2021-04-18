using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.DataEncryption
{
    /// <summary>
    /// Provides extensions for the <see cref="ModelBuilder"/>.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Enables encryption on this model using an encryption provider.
        /// </summary>
        /// <param name="modelBuilder">
        /// The <see cref="ModelBuilder"/> instance.
        /// </param>
        /// <param name="encryptionProvider">
        /// The <see cref="IEncryptionProvider"/> to use, if any.
        /// </param>
        /// <returns>
        /// The updated <paramref name="modelBuilder"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="modelBuilder"/> is <see langword="null"/>.
        /// </exception>
        public static ModelBuilder UseEncryption(this ModelBuilder modelBuilder, IEncryptionProvider encryptionProvider)
        {
            if (modelBuilder is null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            ValueConverter binaryToBinary = null, binaryToString = null;
            ValueConverter stringToBinary = null, stringToString = null;
            ValueConverter secureStringToBinary = null, secureStringToString = null;
            var secureStringProperties = new List<(Type entityType, string propertyName)>();

            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (IMutableProperty property in entityType.GetProperties())
                {
                    var (shouldEncrypt, format) = property.ShouldEncrypt();
                    if (!shouldEncrypt)
                    {
                        continue;
                    }

                    if (property.ClrType == typeof(byte[]))
                    {
                        switch (format)
                        {
                            case StorageFormat.Base64:
                            {
                                binaryToString ??= encryptionProvider.FromBinary().ToBase64().Build();
                                property.SetValueConverter(binaryToString);
                                break;
                            }
                            case StorageFormat.Binary:
                            case StorageFormat.Default:
                            {
                                if (encryptionProvider is not null)
                                {
                                    binaryToBinary ??= encryptionProvider.FromBinary().ToBinary().Build();
                                    property.SetValueConverter(binaryToBinary);
                                }
                                break;
                            }
                            default:
                            {
                                throw new NotSupportedException($"Storage format {format} is not supported.");
                            }
                        }
                    }
                    else if (property.ClrType == typeof(string))
                    {
                        switch (format)
                        {
                            case StorageFormat.Binary:
                            {
                                stringToBinary ??= encryptionProvider.FromString().ToBinary().Build();
                                property.SetValueConverter(stringToBinary);
                                break;
                            }
                            case StorageFormat.Base64:
                            case StorageFormat.Default:
                            {
                                if (encryptionProvider is not null)
                                {
                                    stringToString ??= encryptionProvider.FromString().ToBase64().Build();
                                    property.SetValueConverter(stringToString);
                                }
                                break;
                            }
                            default:
                            {
                                throw new NotSupportedException($"Storage format {format} is not supported.");
                            }
                        }
                    }
                    else if (property.ClrType == typeof(SecureString))
                    {
                        switch (format)
                        {
                            case StorageFormat.Base64:
                            {
                                secureStringToString ??= encryptionProvider.FromSecureString().ToBase64().Build();
                                property.SetValueConverter(secureStringToString);
                                break;
                            }
                            case StorageFormat.Binary:
                            case StorageFormat.Default:
                            {
                                secureStringToBinary ??= encryptionProvider.FromSecureString().ToBinary().Build();
                                property.SetValueConverter(secureStringToBinary);
                                break;
                            }
                            default:
                            {
                                throw new NotSupportedException($"Storage format {format} is not supported.");
                            }
                        }
                    }
                }

                // By default, SecureString properties are created as navigation properties, and need to be reconfigured:
                foreach (var navigation in entityType.GetNavigations())
                {
                    if (navigation.ClrType == typeof(SecureString))
                    {
                        secureStringProperties.Add((entityType.ClrType, navigation.Name));
                    }
                }
            }

            if (secureStringProperties.Count != 0)
            {
                foreach (var (entityType, propertyName) in secureStringProperties)
                {
                    var property = modelBuilder.Entity(entityType).Property(propertyName);
                    var attribute = property.Metadata.PropertyInfo?.GetCustomAttribute<EncryptedAttribute>(false);
                    var format = attribute?.Format ?? StorageFormat.Default;

                    switch (format)
                    {
                        case StorageFormat.Base64:
                        {
                            secureStringToString ??= encryptionProvider.FromSecureString().ToBase64().Build();
                            property.HasConversion(secureStringToString);
                            break;
                        }
                        case StorageFormat.Binary:
                        case StorageFormat.Default:
                        {
                            secureStringToBinary ??= encryptionProvider.FromSecureString().ToBinary().Build();
                            property.HasConversion(secureStringToBinary);
                            break;
                        }
                        default:
                        {
                            throw new NotSupportedException($"Storage format {format} is not supported.");
                        }
                    }
                }
            }

            return modelBuilder;
        }
    }
}
