using Bogus;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Migration
{
    public class MigratorBaseTest
    {
        protected IEnumerable<AuthorEntity> Authors { get; }

        protected MigratorBaseTest()
        {
            var faker = new Faker();
            Authors = Enumerable.Range(0, faker.Random.Byte())
                .Select(x => new AuthorEntity(faker.Name.FirstName(), faker.Name.LastName(), faker.Random.Int(0, 90))
                {
                    Books = Enumerable.Range(0, 10).Select(y => new BookEntity(faker.Lorem.Sentence(), faker.Random.Int(100, 500))).ToList()
                }).ToList();
        }

        protected void AssertAuthor(AuthorEntity expected, AuthorEntity actual)
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
    }
}
