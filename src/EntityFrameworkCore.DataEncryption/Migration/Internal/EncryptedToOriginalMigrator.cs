using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Migration.Internal
{
    internal class EncryptedToOriginalMigrator : IDataMigrator
    {
        public void Migrate(DbContext context, IEnumerable<EncryptedEntity> encryptedEntities)
        {
            throw new System.NotImplementedException();
        }
    }
}
