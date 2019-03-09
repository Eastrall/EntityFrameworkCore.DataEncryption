using Microsoft.EntityFrameworkCore.Encryption.Provider;
using Microsoft.EntityFrameworkCore.Encryption.Test.Context;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Providers
{
    public class AesProviderTest
    {
        [Theory]
        [InlineData("Hello world", AesKeySize.AES128Bits)]
        [InlineData("Hello world", AesKeySize.AES192Bits)]
        [InlineData("Hello world", AesKeySize.AES256Bits)]
        public void EncryptDecryptStringTest(string input, AesKeySize aesKeyType)
        {
            byte[] encryptionKey = AesProvider.GenerateKey(aesKeyType);
            byte[] iv = Enumerable.Repeat<byte>(0, 16).ToArray();
            var provider = new AesProvider(encryptionKey, iv);

            string encryptedData = provider.Encrypt(input);
            Assert.NotNull(encryptedData);

            string decryptedData = provider.Decrypt(encryptedData);
            Assert.NotNull(decryptedData);

            Assert.Equal(input, decryptedData);
        }

        [Theory]
        [InlineData(AesKeySize.AES128Bits)]
        [InlineData(AesKeySize.AES192Bits)]
        [InlineData(AesKeySize.AES256Bits)]
        public void GenerateAesKeyTest(AesKeySize keySize)
        {
            var key = AesProvider.GenerateKey(keySize);

            Assert.NotNull(key);
            Assert.Equal((int)keySize / 8, key.Length);
        }

        [Fact]
        public void EncryptUsingAes128Provider()
        {
            this.ExecuteAesEncryptionTest<Aes128EncryptedDatabaseContext>(AesKeySize.AES128Bits);
        }

        [Fact]
        public void EncryptUsingAes192Provider()
        {
            this.ExecuteAesEncryptionTest<Aes192EncryptedDatabaseContext>(AesKeySize.AES192Bits);
        }

        [Fact]
        public void EncryptUsingAes256Provider()
        {
            this.ExecuteAesEncryptionTest<Aes256EncryptedDatabaseContext>(AesKeySize.AES256Bits);
        }

        private void ExecuteAesEncryptionTest<TContext>(AesKeySize aesKeyType) where TContext : DatabaseContext
        {
            byte[] encryptionKey = AesProvider.GenerateKey(aesKeyType);
            byte[] iv = Enumerable.Repeat<byte>(0, 16).ToArray();
            var provider = new AesProvider(encryptionKey, iv, CipherMode.CBC, PaddingMode.Zeros);
            var author = new AuthorEntity("John", "Doe", 42)
            {
                Books = new List<BookEntity>()
                {
                    new BookEntity("Lorem Ipsum", 300),
                    new BookEntity("Dolor sit amet", 390)
                }
            };
            var authorEncryptedFirstName = provider.Encrypt(author.FirstName);
            var authorEncryptedLastName = provider.Encrypt(author.LastName);
            var firstBookEncryptedName = provider.Encrypt(author.Books.First().Name);
            var lastBookEncryptedName = provider.Encrypt(author.Books.Last().Name);

            using (var contextFactory = new DatabaseContextFactory())
            {
                // Save data to an encrypted database context
                using (var dbContext = contextFactory.CreateContext<TContext>(provider))
                {
                    dbContext.Authors.Add(author);
                    dbContext.SaveChanges();
                }

                // Read encrypted data from normal context and compare with encrypted data.
                using (var dbContext = contextFactory.CreateContext<DatabaseContext>())
                {
                    var authorFromDb = dbContext.Authors.Include(x => x.Books).FirstOrDefault();

                    Assert.NotNull(authorFromDb);
                    Assert.Equal(authorEncryptedFirstName, authorFromDb.FirstName);
                    Assert.Equal(authorEncryptedLastName, authorFromDb.LastName);
                    Assert.NotNull(authorFromDb.Books);
                    Assert.NotEmpty(authorFromDb.Books);
                    Assert.Equal(2, authorFromDb.Books.Count);
                    Assert.Equal(firstBookEncryptedName, authorFromDb.Books.First().Name);
                    Assert.Equal(lastBookEncryptedName, authorFromDb.Books.Last().Name);
                }

                // Read decrypted data and compare with original data
                using (var dbContext = contextFactory.CreateContext<TContext>(provider))
                {
                    var authorFromDb = dbContext.Authors.Include(x => x.Books).FirstOrDefault();

                    Assert.NotNull(authorFromDb);
                    Assert.Equal(author.FirstName, authorFromDb.FirstName);
                    Assert.Equal(author.LastName, authorFromDb.LastName);
                    Assert.NotNull(authorFromDb.Books);
                    Assert.NotEmpty(authorFromDb.Books);
                    Assert.Equal(2, authorFromDb.Books.Count);
                    Assert.Equal(author.Books.First().Name, authorFromDb.Books.First().Name);
                    Assert.Equal(author.Books.Last().Name, authorFromDb.Books.Last().Name);
                }
            }
        }

        public class Aes128EncryptedDatabaseContext : DatabaseContext
        {
            public Aes128EncryptedDatabaseContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
                : base(options, encryptionProvider) { }
        }

        public class Aes192EncryptedDatabaseContext : DatabaseContext
        {
            public Aes192EncryptedDatabaseContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
                : base(options, encryptionProvider) { }
        }

        public class Aes256EncryptedDatabaseContext : DatabaseContext
        {
            public Aes256EncryptedDatabaseContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
                : base(options, encryptionProvider) { }
        }
    }
}
