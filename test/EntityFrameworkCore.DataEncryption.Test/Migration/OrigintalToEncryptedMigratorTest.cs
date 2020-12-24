using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Migration;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Migration
{
    public class OrigintalToEncryptedMigratorTest : MigratorBaseTest
    {
        [Fact]
        public void MigrateOriginalToEncryptedTest()
        {
            var aesKeys = AesProvider.GenerateKey(AesKeySize.AES256Bits);
            var provider = new AesProvider(aesKeys.Key);
            string databaseName = Guid.NewGuid().ToString();

            // Feed database with data.
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<MigrationRawOriginalContext>();

                context.Authors.AddRange(Authors);
                context.SaveChanges();
            }

            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<MigrationTransitionDbContext>();
                var migrator = new DataMigrator<MigrationTransitionDbContext>(context);

                migrator.MigrateOriginalToEncrypted(provider);
            }

            // Assert if the context has been encrypted
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<MigrationEncryptedContext>(provider);
                IEnumerable<AuthorEntity> authors = context.Authors.Include(x => x.Books);

                foreach (AuthorEntity author in authors)
                {
                    AuthorEntity original = Authors.FirstOrDefault(x => x.UniqueId == author.UniqueId);

                    AssertAuthor(original, author);
                }
            }

            // Assert data context if raw data is encrypted
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<MigrationRawOriginalContext>(provider);
                IEnumerable<AuthorEntity> authors = context.Authors.Include(x => x.Books);

                foreach (AuthorEntity author in authors)
                {
                    AuthorEntity original = Authors.FirstOrDefault(x => x.UniqueId == author.UniqueId);

                    Assert.NotNull(original);
                    Assert.Equal(original.FirstName, provider.Decrypt(author.FirstName));
                    Assert.Equal(original.LastName, provider.Decrypt(author.LastName));
                    Assert.Equal(original.Books.Count, author.Books.Count);

                    foreach (BookEntity actualBook in author.Books)
                    {
                        BookEntity expectedBook = original.Books.FirstOrDefault(x => x.UniqueId == actualBook.UniqueId);

                        Assert.NotNull(expectedBook);
                        Assert.Equal(expectedBook.Name, provider.Decrypt(actualBook.Name));
                        Assert.Equal(expectedBook.NumberOfPages, actualBook.NumberOfPages);
                    }
                }
            }

            File.Delete(databaseName);
        }

        private class MigrationRawOriginalContext : MigrationContext
        {
            public MigrationRawOriginalContext(DbContextOptions options) 
                : base(options)
            {
            }

            public MigrationRawOriginalContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null) 
                : base(options, encryptionProvider)
            {
            }
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

        private class MigrationEncryptedContext : MigrationContext
        {
            public MigrationEncryptedContext(DbContextOptions options) : base(options)
            {
            }

            public MigrationEncryptedContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null) 
                : base(options, encryptionProvider)
            {
            }
        }
    }
}
