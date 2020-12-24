using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Migration;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers.Obsolete;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Migration
{
    public class V1ToV2MigratorTest : MigratorBaseTest
    {
        [Fact]
        public void MigrateV1ToV2Test()
        {
            string databaseName = $"{Guid.NewGuid()}.db";
            var aesKeys = AesProvider.GenerateKey(AesKeySize.AES256Bits);

            // Feed database with V1 encrypted data.
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<DatabaseContext>(new AesProviderV1(aesKeys.Key, aesKeys.IV));

                context.Authors.AddRange(Authors);
                context.SaveChanges();
            }

            // Process migration
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<MigrationTransitionDbContext>();
                var migrator = new DataMigrator<MigrationTransitionDbContext>(context);

                migrator.MigrateAesV1ToV2(new AesProvider(aesKeys.Key), aesKeys.Key, aesKeys.IV);
            }

            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<MigrationContext>(new AesProvider(aesKeys.Key));
                IEnumerable<AuthorEntity> authors = context.Authors.Include(x => x.Books);

                foreach (AuthorEntity author in authors)
                {
                    AuthorEntity original = Authors.FirstOrDefault(x => x.UniqueId == author.UniqueId);

                    AssertAuthor(original, author);
                }
            }

            File.Delete(databaseName);
        }

        private class MigrationTransitionDbContext : MigrationContext
        {
            public MigrationTransitionDbContext(DbContextOptions options) : base(options)
            {
            }

            public MigrationTransitionDbContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
                : base(options, encryptionProvider)
            {
            }
        }
    }
}
