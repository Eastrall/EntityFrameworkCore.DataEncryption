using Microsoft.EntityFrameworkCore.Encryption.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Encryption
{
    public static class ModelBuilderExtensions
    {
        public static void UseEncryption(this ModelBuilder modelBuilder, IEncryptionProvider encryptionProvider)
        {
            if (encryptionProvider == null)
                return;

            var encryptionConverter = new EncryptionConverter(encryptionProvider);

            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (IMutableProperty property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        object[] attributes = property.PropertyInfo.GetCustomAttributes(typeof(EncryptedAttribute), false);

                        if (attributes.Any())
                            property.SetValueConverter(encryptionConverter);
                    }
                }
            }
        }
    }

    public interface IEncryptionProvider
    {
        string Encrypt(string dataToEncrypt);

        string Decrypt(string dataToDecrypt);
    }
}
