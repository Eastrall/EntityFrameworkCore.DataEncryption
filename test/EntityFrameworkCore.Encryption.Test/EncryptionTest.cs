using Microsoft.EntityFrameworkCore.Encryption.Provider;
using Microsoft.EntityFrameworkCore.Encryption.Test.Context;
using System.Linq;
using System.Text;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test
{
    public sealed class EncryptionTest
    {
        private const string InMemoryDatabaseName = "inMemoryDatabase";
        private readonly DbContextOptions<DatabaseContext> _databaseContextOptions;

        public EncryptionTest()
        {
            this._databaseContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: InMemoryDatabaseName)
                .Options;
        }

        [Fact]
        public void CreateDatabaseContext()
        {
            var dbContext = new DatabaseContext();

            Assert.NotNull(dbContext);

            dbContext.Dispose();
        }

        [Fact]
        public void CreateDatabaseInMemoryContext()
        {
            var dbContext = new DatabaseContext(this._databaseContextOptions);

            Assert.NotNull(dbContext);

            dbContext.Dispose();
        }

        [Fact]
        public void EncryptUsingAesProvider()
        {
            const string encryptionKey = "5gzlpRQIceJazRG67pywnPDnfcR90ACFFeJRcWLKi+c=";
            byte[] iv = Enumerable.Repeat<byte>(0, 16).ToArray();

            var provider = new AesProvider(Encoding.UTF8.GetBytes(encryptionKey), iv);

            using (var dbContext = new DatabaseContext(this._databaseContextOptions, provider))
            {
                // TODO: add logic
                dbContext.SaveChanges();
            }
        }
    }
}
