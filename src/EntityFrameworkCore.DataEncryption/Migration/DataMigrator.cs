using Microsoft.EntityFrameworkCore.DataEncryption.Migration.Internal;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers.Obsolete;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Migration
{
    /// <summary>
    /// Provides a mechanism to migrate data.
    /// </summary>
    public sealed class DataMigrator<TContext> where TContext : DbContext
    {
        private readonly TContext _databaseContext;

        /// <summary>
        /// Creates a new <see cref="DataMigrator{TContext}"/> instance.
        /// </summary>
        /// <param name="databaseContext">Database context.</param>
        public DataMigrator(TContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        /// <summary>
        /// Process a data migration to migrate plain text data into encrypted data.
        /// </summary>
        /// <param name="encryptionProvider">Encryption provider.</param>
        public void MigrateOriginalToEncrypted(IEncryptionProvider encryptionProvider)
        {
            MigrateData(new OriginalToEncryptedMigrator(encryptionProvider));
        }

        /// <summary>
        /// Process a data migration to migrate encryted data into plain text data.
        /// </summary>
        /// <param name="encryptionProvider">Encryption provider.</param>
        public void MigrateEncryptedToOriginal(IEncryptionProvider encryptionProvider)
        {
            MigrateData(new EncryptedToOriginalMigrator(encryptionProvider));
        }

        /// <summary>
        /// Migrates the data from V1 to V2.
        /// </summary>
        /// <param name="encryptionProvider">V2 encryption provider</param>
        /// <param name="encryptionKey">V1 encryption key.</param>
        /// <param name="initializationVector">V1 initialization vector.</param>
        /// <param name="mode">Mode for operation used in the symetric encryption.</param>
        /// <param name="padding">Padding mode used in the symetric encryption.</param>
        public void MigrateAesV1ToV2(IEncryptionProvider encryptionProvider, byte[] encryptionKey, byte[] initializationVector, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
        {
            var aesProviderV1 = new AesProviderV1(encryptionKey, initializationVector, mode, padding);
            var migrator = new V1ToV2DataMigrator(aesProviderV1, encryptionProvider);
            
            MigrateData(migrator);
        }

        /// <summary>
        /// Procress the data migration according to the given data migrator.
        /// </summary>
        /// <param name="dataMigrator"></param>
        private void MigrateData(IDataMigrator dataMigrator)
        {
            IEnumerable<EncryptedEntity> encryptedEntities = _databaseContext.Model.GetEntityTypes()
                .Select(x => x.ClrType)
                .Where(x => x.GetProperties().Any(p => p.GetCustomAttribute<EncryptedAttribute>() != null))
                .Select(x => 
                    new EncryptedEntity(
                        x,
                        x.GetProperties().Where(p => p.GetCustomAttribute<EncryptedAttribute>() != null)
                    )
                );

            dataMigrator.Migrate(_databaseContext, encryptedEntities);
        }
    }
}
