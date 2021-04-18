using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.DataEncryption
{
    /// <summary>
    /// Extension methods for EF models.
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Returns a value indicating whether the specified property should be encrypted.
        /// </summary>
        /// <param name="property">
        /// The <see cref="IProperty"/>.
        /// </param>
        /// <returns>
        /// A value indicating whether the specified property should be encrypted,
        /// and how the encrypted value should be stored.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is <see langword="null"/>.
        /// </exception>
        public static (bool shouldEncrypt, StorageFormat format) ShouldEncrypt(this IProperty property)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var attribute = property.PropertyInfo?.GetCustomAttribute<EncryptedAttribute>(false);
            if (property.ClrType == typeof(SecureString))
            {
                return (true, attribute?.Format ?? StorageFormat.Binary);
            }

            return attribute is null ? (false, StorageFormat.Default) : (true, attribute.Format);
        }

        /// <summary>
        /// Returns a value indicating whether the specified property should be encrypted.
        /// </summary>
        /// <param name="property">
        /// The <see cref="IMutableProperty"/>.
        /// </param>
        /// <returns>
        /// A value indicating whether the specified property should be encrypted,
        /// and how the encrypted value should be stored.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is <see langword="null"/>.
        /// </exception>
        public static (bool shouldEncrypt, StorageFormat format) ShouldEncrypt(this IMutableProperty property)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

#pragma warning disable EF1001 // Internal EF Core API usage.
            if (property.FindAnnotation(CoreAnnotationNames.ValueConverter) is not null)
            {
                return (false, StorageFormat.Default);
            }
#pragma warning restore EF1001 // Internal EF Core API usage.

            var attribute = property.PropertyInfo?.GetCustomAttribute<EncryptedAttribute>(false);
            if (property.ClrType == typeof(SecureString))
            {
                return (true, attribute?.Format ?? StorageFormat.Binary);
            }

            return attribute is null ? (false, StorageFormat.Default) : (true, attribute.Format);
        }

        /// <summary>
        /// Returns the list of encrypted properties for the specified entity type.
        /// </summary>
        /// <param name="entityType">
        /// The <see cref="IEntityType"/>.
        /// </param>
        /// <returns>
        /// A list of the properties for the specified type which should be encrypted.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityType"/> is <see langword="null"/>.
        /// </exception>
        public static IReadOnlyList<(IProperty property, StorageFormat format)> ListEncryptedProperties(this IEntityType entityType)
        {
            if (entityType is null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            return entityType.GetProperties()
                .Select(p => (property: p, flag: p.ShouldEncrypt()))
                .Where(p => p.flag.shouldEncrypt)
                .Select(p => (p.property, p.flag.format)).ToList();
        }
    }
}