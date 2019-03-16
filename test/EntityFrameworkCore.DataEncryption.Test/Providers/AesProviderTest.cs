using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Xunit;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Providers
{
    public class AesProviderTest
    {
        [Theory]
        [InlineData(AesKeySize.AES128Bits)]
        [InlineData(AesKeySize.AES192Bits)]
        [InlineData(AesKeySize.AES256Bits)]
        public void EncryptDecryptStringTest(AesKeySize keySize)
        {
            string input = StringHelper.RandomString(20);
            AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(keySize);
            var provider = new AesProvider(encryptionKeyInfo.Key, encryptionKeyInfo.IV);

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
            AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(keySize);

            Assert.NotNull(encryptionKeyInfo.Key);
            Assert.NotNull(encryptionKeyInfo.IV);
            Assert.Equal((int)keySize / 8, encryptionKeyInfo.Key.Length);
        }

        [Theory]
        [InlineData(AesKeySize.AES128Bits)]
        [InlineData(AesKeySize.AES192Bits)]
        [InlineData(AesKeySize.AES256Bits)]
        public void CompareTwoAesKeysInstancesTest(AesKeySize keySize)
        {
            AesKeyInfo encryptionKeyInfo1 = AesProvider.GenerateKey(keySize);
            AesKeyInfo encryptionKeyInfo2 = AesProvider.GenerateKey(keySize);
            AesKeyInfo encryptionKeyInfoCopy = encryptionKeyInfo1;

            Assert.NotNull(encryptionKeyInfo1.Key);
            Assert.NotNull(encryptionKeyInfo1.IV);
            Assert.NotNull(encryptionKeyInfo2.Key);
            Assert.NotNull(encryptionKeyInfo2.IV);
            Assert.True(encryptionKeyInfo1 == encryptionKeyInfoCopy);
            Assert.True(encryptionKeyInfo1.Equals(encryptionKeyInfoCopy));
            Assert.True(encryptionKeyInfo1 != encryptionKeyInfo2);
            Assert.True(encryptionKeyInfo1.GetHashCode() != encryptionKeyInfo2.GetHashCode());
            Assert.False(encryptionKeyInfo1.Equals(0));
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
            AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(aesKeyType);
            var provider = new AesProvider(encryptionKeyInfo.Key, encryptionKeyInfo.IV, CipherMode.CBC, PaddingMode.Zeros);
            var author = new AuthorEntity("John", "Doe", 42)
            {
                Books = new List<BookEntity>()
                {
                    new BookEntity("Lorem Ipsum", 300),
                    new BookEntity("Dolor sit amet", 390)
                }
            };
            string authorEncryptedFirstName = provider.Encrypt(author.FirstName);
            string authorEncryptedLastName = provider.Encrypt(author.LastName);
            string firstBookEncryptedName = provider.Encrypt(author.Books.First().Name);
            string lastBookEncryptedName = provider.Encrypt(author.Books.Last().Name);

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
