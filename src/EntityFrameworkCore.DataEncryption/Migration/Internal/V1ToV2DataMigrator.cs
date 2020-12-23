using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Migration.Internal
{
    internal class V1ToV2DataMigrator : DataMigratorBase, IDataMigrator
    {
        private readonly IEncryptionProvider _sourceEncryptionProvider;
        private readonly IEncryptionProvider _encryptionProvider;

        /// <summary>
        /// Creates a new <see cref="V1ToV2DataMigrator"/> instance with the V1 encryption provider and the new encryption provider.
        /// </summary>
        /// <param name="sourceEncryptionProvider">V1 encryption provider.</param>
        /// <param name="encryptionProvider">V2 encryption provider.</param>
        public V1ToV2DataMigrator(IEncryptionProvider sourceEncryptionProvider, IEncryptionProvider encryptionProvider)
        {
            _sourceEncryptionProvider = sourceEncryptionProvider ?? throw new ArgumentNullException(nameof(sourceEncryptionProvider));
            _encryptionProvider = encryptionProvider ?? throw new ArgumentNullException(nameof(encryptionProvider));
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
                var entities = GetDbSet(context, encryptedEntity.EntityType);

                foreach (object entity in entities)
                {
                    foreach (PropertyInfo entityProperty in encryptedEntity.EncryptedProperties)
                    {
                        string sourceValue = entityProperty.GetValue(entity)?.ToString();

                        if (sourceValue is null)
                        {
                            continue;
                        }

                        string decryptedValue = _sourceEncryptionProvider.Decrypt(sourceValue);
                        string newValue = _encryptionProvider.Encrypt(decryptedValue);

                        entityProperty.SetValue(entity, newValue);
                    }
                }

                context.SaveChanges();
            }
        }
    }
}
