using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Migration.Internal
{
    internal class OriginalToEncryptedMigrator : DataMigratorBase, IDataMigrator
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public OriginalToEncryptedMigrator(IEncryptionProvider encryptionProvider)
        {
            _encryptionProvider = encryptionProvider;
        }

        public void Migrate(DbContext context, IEnumerable<EncryptedEntity> encryptedEntities)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (encryptedEntities is null)
            {
                throw new ArgumentNullException(nameof(encryptedEntities));
            }

            foreach (EncryptedEntity encryptedEntity in encryptedEntities)
            {
                IEnumerable<object> entities = GetDbSet(context, encryptedEntity.EntityType);

                foreach (object entity in entities)
                {
                    foreach (PropertyInfo entityProperty in encryptedEntity.EncryptedProperties)
                    {
                        string sourceValue = entityProperty.GetValue(entity)?.ToString();

                        if (sourceValue is null)
                        {
                            continue;
                        }

                        string newValue = _encryptionProvider.Encrypt(sourceValue);

                        entityProperty.SetValue(entity, newValue);
                    }
                }

                context.SaveChanges();
            }
        }
    }
}
