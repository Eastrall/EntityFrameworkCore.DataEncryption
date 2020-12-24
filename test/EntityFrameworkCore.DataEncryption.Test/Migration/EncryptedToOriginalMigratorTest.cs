using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Migration;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Migration
{
    public class EncryptedToOriginalMigratorTest : MigratorBaseTest
    {
        [Fact]
        public void MigrateEncryptedToOriginalTest()
        {
            var aesKeys = AesProvider.GenerateKey(AesKeySize.AES256Bits);
            var provider = new AesProvider(aesKeys.Key);
            string databaseName = Guid.NewGuid().ToString();

            // Feed database with data.
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<EncryptedToOriginalMigrationEncryptedContext>(provider);

                context.Authors.AddRange(Authors);
                context.SaveChanges();
            }

            // Process data migration
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<EncryptedToOriginalMigrationTransitionContext>();
                var migrator = new DataMigrator<EncryptedToOriginalMigrationTransitionContext>(context);

                migrator.MigrateEncryptedToOriginal(provider);
            }

            // Assert if the context has been decrypted
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<EncryptedToOriginalMigrationContext>();
                IEnumerable<AuthorEntity> authors = context.Authors.Include(x => x.Books);

                foreach (AuthorEntity author in authors)
                {
                    AuthorEntity original = Authors.FirstOrDefault(x => x.UniqueId == author.UniqueId);

                    AssertAuthor(original, author);
                }
            }
        }

        private class EncryptedToOriginalMigrationContext : MigrationContext
        {
            public EncryptedToOriginalMigrationContext(DbContextOptions options)
                : base(options)
            {
            }

            public EncryptedToOriginalMigrationContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
                : base(options, encryptionProvider)
            {
            }
        }

        private class EncryptedToOriginalMigrationTransitionContext : MigrationContext
        {
            public EncryptedToOriginalMigrationTransitionContext(DbContextOptions options) : base(options)
            {
            }

            public EncryptedToOriginalMigrationTransitionContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
                : base(options, encryptionProvider)
            {
            }
        }

        private class EncryptedToOriginalMigrationEncryptedContext : MigrationContext
        {
            public EncryptedToOriginalMigrationEncryptedContext(DbContextOptions options) : base(options)
            {
            }

            public EncryptedToOriginalMigrationEncryptedContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
                : base(options, encryptionProvider)
            {
            }
        }
    }
}
