using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore.DataEncryption.Migration;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Migration
{
    public abstract class MigratorBaseTest
    {
        private IEnumerable<AuthorEntity> Authors { get; }

        protected MigratorBaseTest()
        {
            var faker = new Faker();
            Authors = Enumerable.Range(0, faker.Random.Byte())
                .Select(_ => new AuthorEntity(faker.Name.FirstName(), faker.Name.LastName(), faker.Random.Int(0, 90))
                {
                    Books = Enumerable.Range(0, 10).Select(_ => new BookEntity(faker.Lorem.Sentence(), faker.Random.Int(100, 500))).ToList()
                }).ToList();
        }

        private static void AssertAuthor(AuthorEntity expected, AuthorEntity actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.Age, actual.Age);
            Assert.Equal(expected.Books.Count, actual.Books.Count);

            foreach (BookEntity actualBook in expected.Books)
            {
                BookEntity expectedBook = actual.Books.FirstOrDefault(x => x.UniqueId == actualBook.UniqueId);

                Assert.NotNull(expectedBook);
                Assert.Equal(expectedBook.Name, actualBook.Name);
                Assert.Equal(expectedBook.NumberOfPages, actualBook.NumberOfPages);
            }
        }

        protected async Task Execute(MigrationEncryptionProvider provider)
        {
            string databaseName = Guid.NewGuid().ToString();

            // Feed database with data.
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                await using var context = contextFactory.CreateContext<DatabaseContext>(provider.SourceEncryptionProvider);
                await context.Authors.AddRangeAsync(Authors);
                await context.SaveChangesAsync();
            }

            // Process data migration
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                await using var context = contextFactory.CreateContext<DatabaseContext>(provider);
                await context.MigrateAsync();
            }

            // Assert if the context has been decrypted
            using (var contextFactory = new DatabaseContextFactory(databaseName))
            {
                await using var context = contextFactory.CreateContext<DatabaseContext>(provider.DestinationEncryptionProvider);
                IEnumerable<AuthorEntity> authors = await context.Authors.Include(x => x.Books).ToListAsync();

                foreach (AuthorEntity author in authors)
                {
                    AuthorEntity original = Authors.FirstOrDefault(x => x.UniqueId == author.UniqueId);
                    AssertAuthor(original, author);
                }
            }
        }
    }
}