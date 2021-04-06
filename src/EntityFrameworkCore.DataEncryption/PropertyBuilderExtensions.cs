using System;
using System.ComponentModel.DataAnnotations;
using System.Security;
using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.DataEncryption
{
    /// <summary>
    /// Provides extensions for the <see cref="PropertyBuilder"/>.
    /// </summary>
    public static class PropertyBuilderExtensions
    {
        /// <summary>
        /// Configures the property as capable of storing encrypted data.
        /// </summary>
        /// <param name="property">
        /// The <see cref="PropertyBuilder{TProperty}"/>.
        /// </param>
        /// <param name="encryptionProvider">
        /// The <see cref="IEncryptionProvider"/> to use, if any.
        /// </param>
        /// <param name="format">
        /// One of the <see cref="StorageFormat"/> values indicating how the value should be stored in the database.
        /// </param>
        /// <param name="mappingHints">
        /// The <see cref="ConverterMappingHints"/> to use, if any.
        /// </param>
        /// <returns>
        /// The updated <see cref="PropertyBuilder{TProperty}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="format"/> is not a recognised value.
        /// </exception>
        public static PropertyBuilder<byte[]> IsEncrypted(
            this PropertyBuilder<byte[]> property,
            IEncryptionProvider encryptionProvider,
            StorageFormat format = StorageFormat.Default,
            ConverterMappingHints mappingHints = null)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return format switch
            {
                StorageFormat.Default => encryptionProvider is null ? property : property.HasConversion(encryptionProvider.FromBinary().ToBinary().Build(mappingHints)),
                StorageFormat.Binary => encryptionProvider is null ? property : property.HasConversion(encryptionProvider.FromBinary().ToBinary().Build(mappingHints)),
                StorageFormat.Base64 => property.HasConversion(encryptionProvider.FromBinary().ToBase64().Build(mappingHints)),
                _ => throw new ArgumentOutOfRangeException(nameof(format)),
            };
        }

        /// <summary>
        /// Configures the property as capable of storing encrypted data.
        /// </summary>
        /// <param name="property">
        /// The <see cref="PropertyBuilder{TProperty}"/>.
        /// </param>
        /// <param name="encryptionProvider">
        /// The <see cref="IEncryptionProvider"/> to use, if any.
        /// </param>
        /// <param name="format">
        /// One of the <see cref="StorageFormat"/> values indicating how the value should be stored in the database.
        /// </param>
        /// <param name="mappingHints">
        /// The <see cref="ConverterMappingHints"/> to use, if any.
        /// </param>
        /// <returns>
        /// The updated <see cref="PropertyBuilder{TProperty}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="format"/> is not a recognised value.
        /// </exception>
        public static PropertyBuilder<string> IsEncrypted(
            this PropertyBuilder<string> property,
            IEncryptionProvider encryptionProvider,
            StorageFormat format = StorageFormat.Default,
            ConverterMappingHints mappingHints = null)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return format switch
            {
                StorageFormat.Default => encryptionProvider is null ? property : property.HasConversion(encryptionProvider.FromString().ToBase64().Build(mappingHints)),
                StorageFormat.Base64 => encryptionProvider is null ? property : property.HasConversion(encryptionProvider.FromString().ToBase64().Build(mappingHints)),
                StorageFormat.Binary => property.HasConversion(encryptionProvider.FromString().ToBinary().Build(mappingHints)),
                _ => throw new ArgumentOutOfRangeException(nameof(format)),
            };
        }

        /// <summary>
        /// Configures the property as capable of storing encrypted data.
        /// </summary>
        /// <param name="property">
        /// The <see cref="PropertyBuilder{TProperty}"/>.
        /// </param>
        /// <param name="encryptionProvider">
        /// The <see cref="IEncryptionProvider"/> to use, if any.
        /// </param>
        /// <param name="format">
        /// One of the <see cref="StorageFormat"/> values indicating how the value should be stored in the database.
        /// </param>
        /// <param name="mappingHints">
        /// The <see cref="ConverterMappingHints"/> to use, if any.
        /// </param>
        /// <returns>
        /// The updated <see cref="PropertyBuilder{TProperty}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="format"/> is not a recognised value.
        /// </exception>
        public static PropertyBuilder<SecureString> IsEncrypted(
            this PropertyBuilder<SecureString> property,
            IEncryptionProvider encryptionProvider,
            StorageFormat format = StorageFormat.Default,
            ConverterMappingHints mappingHints = null)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return format switch
            {
                StorageFormat.Default => property.HasConversion(encryptionProvider.FromSecureString().ToBinary().Build(mappingHints)),
                StorageFormat.Binary => property.HasConversion(encryptionProvider.FromSecureString().ToBinary().Build(mappingHints)),
                StorageFormat.Base64 => property.HasConversion(encryptionProvider.FromSecureString().ToBase64().Build(mappingHints)),
                _ => throw new ArgumentOutOfRangeException(nameof(format)),
            };
        }
    }
}