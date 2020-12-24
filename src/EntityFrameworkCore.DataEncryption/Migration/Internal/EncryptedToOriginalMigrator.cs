using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Migration.Internal
{
    internal class EncryptedToOriginalMigrator : IDataMigrator
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public EncryptedToOriginalMigrator(IEncryptionProvider encryptionProvider)
        {
            _encryptionProvider = encryptionProvider;
        }

        public void Migrate(DbContext context, IEnumerable<EncryptedEntity> encryptedEntities)
        {
            throw new System.NotImplementedException();
        }
    }
}
