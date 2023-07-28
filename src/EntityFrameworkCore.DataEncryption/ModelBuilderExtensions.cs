using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.DataEncryption.Serialization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.DataEncryption;

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
    public static ModelBuilder UseEncryption(this ModelBuilder modelBuilder,
        IEncryptionProvider encryptionProvider,
        ISerializationProvider serializationProvider = null)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        if (encryptionProvider is null)
        {
            throw new ArgumentNullException(nameof(encryptionProvider));
        }

        if (serializationProvider is null)
        {
            serializationProvider = new BinarySerializationProvider();
        }

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            IEnumerable<EncryptedProperty> encryptedProperties = GetEntityEncryptedProperties(entityType);

            foreach (EncryptedProperty encryptedProperty in encryptedProperties)
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                if (encryptedProperty.Property.FindAnnotation(CoreAnnotationNames.ValueConverter) is not null)
                {
                    continue;
                }
#pragma warning restore EF1001 // Internal EF Core API usage.

                ValueConverter converter = GetValueConverter(encryptedProperty.Property.ClrType, encryptionProvider, serializationProvider, encryptedProperty.StorageFormat);

                if (converter != null)
                {
                    encryptedProperty.Property.SetValueConverter(converter);
                }
            }
        }

        return modelBuilder;
    }

    private static ValueConverter GetValueConverter(Type propertyType, IEncryptionProvider encryptionProvider, ISerializationProvider serializationProvider, StorageFormat storageFormat)
    {
        MethodInfo method = typeof(ModelBuilderExtensions).GetMethod(nameof(GetGenericValueConverter), BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo generic = method.MakeGenericMethod(propertyType);
        return (ValueConverter)generic.Invoke(null,  new object[] { encryptionProvider, serializationProvider, storageFormat });
    }

    private static ValueConverter GetGenericValueConverter<TModel>(IEncryptionProvider encryptionProvider, ISerializationProvider serializationProvider, StorageFormat storageFormat)
    {
        return storageFormat switch
        {
            StorageFormat.Default or StorageFormat.Binary =>
                new EncryptionConverter<TModel, byte[]>(encryptionProvider, serializationProvider, new ByteArraySerializationProvider()),
            StorageFormat.Base64 => 
                new EncryptionConverter<TModel, string>(encryptionProvider, serializationProvider, new Base64SerializationProvider()),
            _ => throw new NotImplementedException()
        };
    }

    private static IEnumerable<EncryptedProperty> GetEntityEncryptedProperties(IMutableEntityType entity)
    {
        return entity.GetProperties()
            .Select(x => EncryptedProperty.Create(x))
            .Where(x => x is not null);
    }

    internal class EncryptedProperty
    {
        public IMutableProperty Property { get; }

        public StorageFormat StorageFormat { get; }

        private EncryptedProperty(IMutableProperty property, StorageFormat storageFormat)
        {
            Property = property;
            StorageFormat = storageFormat;
        }

        public static EncryptedProperty Create(IMutableProperty property)
        {
            StorageFormat? storageFormat = null;

            var encryptedAttribute = property.PropertyInfo?.GetCustomAttribute<EncryptedAttribute>(false);

            if (encryptedAttribute != null)
            {
                storageFormat = encryptedAttribute.Format;
            }

            IAnnotation encryptedAnnotation = property.FindAnnotation(PropertyAnnotations.IsEncrypted);

            if (encryptedAnnotation != null && (bool)encryptedAnnotation.Value)
            {
                storageFormat = (StorageFormat)property.FindAnnotation(PropertyAnnotations.StorageFormat)?.Value;
            }

            return storageFormat.HasValue ? new EncryptedProperty(property, storageFormat.Value) : null;
        }
    }
}
