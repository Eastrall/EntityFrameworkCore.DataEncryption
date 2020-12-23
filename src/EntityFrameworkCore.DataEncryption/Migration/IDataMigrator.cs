using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Migration
{
    internal interface IDataMigrator
    {
        void Migrate(DbContext context, IEnumerable<EncryptedEntity> encryptedEntities);
    }
}
