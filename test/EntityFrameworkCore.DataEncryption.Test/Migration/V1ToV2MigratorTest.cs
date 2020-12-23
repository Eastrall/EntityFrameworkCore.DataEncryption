using Bogus;
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
    public class V1ToV2MigratorTest
    {
        private readonly IEnumerable<AuthorEntity> _authors;

        public V1ToV2MigratorTest()
        {
            var faker = new Faker();
            _authors = Enumerable.Range(0, faker.Random.Byte())
                .Select(x => new AuthorEntity(faker.Name.FirstName(), faker.Name.LastName(), faker.Random.Int(0, 90))
                {
                    Books = Enumerable.Range(0, 10)
                                .Select(y => new BookEntity(faker.Lorem.Sentence(), faker.Random.Int(100, 500))
                                {
                                }).ToList()
                }).ToList();
        }

        [Fact]
        public void MigrateV1ToV2Test()
        {
            string databaseName = $"{Guid.NewGuid()}.db";
            var aesKeys = AesProvider.GenerateKey(AesKeySize.AES256Bits);

            // Feed database with V1 encrypted data.
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<DatabaseContext>(new AesProviderV1(aesKeys.Key, aesKeys.IV));

                context.Authors.AddRange(_authors);
                context.SaveChanges();
            }

            // Process migration
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<TransitionDbContext>();
                var migrator = new DataMigrator<TransitionDbContext>(context);

                migrator.MigrateAesV1ToV2(new AesProvider(aesKeys.Key), aesKeys.Key, aesKeys.IV);
            }

            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                using var context = contextFactory.CreateContext<MigrationContext>(new AesProvider(aesKeys.Key));
                IEnumerable<AuthorEntity> authors = context.Authors.Include(x => x.Books);

                foreach (AuthorEntity author in authors)
                {
                    AuthorEntity original = _authors.FirstOrDefault(x => x.UniqueId == author.UniqueId);

                    Assert.NotNull(original);
                    Assert.Equal(original.FirstName, author.FirstName);
                    Assert.Equal(original.LastName, author.LastName);
                    Assert.Equal(original.Age, author.Age);
                    Assert.Equal(original.Books.Count, author.Books.Count);

                    foreach (BookEntity book in author.Books)
                    {
                        BookEntity originalBook = original.Books.FirstOrDefault(x => x.UniqueId == book.UniqueId);

                        Assert.NotNull(originalBook);
                        Assert.Equal(originalBook.Name, book.Name);
                        Assert.Equal(originalBook.NumberOfPages, book.NumberOfPages);
                    }
                }
            }

            File.Delete(databaseName);
        }

        public class TransitionDbContext : MigrationContext
        {
            public TransitionDbContext(DbContextOptions options) : base(options)
            {
            }

            public TransitionDbContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null) 
                : base(options, encryptionProvider)
            {
            }
        }
    }
}
