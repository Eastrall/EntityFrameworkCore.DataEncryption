using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.DataEncryption
{
    /// <summary>
    /// Provides extensions for the <see cref="ModelBuilder"/>.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Enables string encryption on this model using an encryption provider.
        /// </summary>
        /// <param name="modelBuilder">Current <see cref="ModelBuilder"/> instance.</param>
        /// <param name="encryptionProvider">Encryption provider.</param>
        public static void UseEncryption(this ModelBuilder modelBuilder, IEncryptionProvider encryptionProvider)
        {
            if (encryptionProvider is null)
            {
                throw new ArgumentNullException(nameof(encryptionProvider), "Cannot initialize encryption with a null provider.");
            }

            var encryptionConverter = new EncryptionConverter(encryptionProvider);

            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (IMutableProperty property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string) && !IsDiscriminator(property))
                    {
                        object[] attributes = property.PropertyInfo.GetCustomAttributes(typeof(EncryptedAttribute), false);

                        if (attributes.Any())
                        {
                            property.SetValueConverter(encryptionConverter);
                        }
                    }
                }
            }
        }

        private static bool IsDiscriminator(IMutableProperty property)
        {
            return property.Name == "Discriminator" || property.PropertyInfo == null;
        }
    }
}
