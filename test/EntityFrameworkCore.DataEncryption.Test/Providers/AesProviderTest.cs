using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Providers
{
    public class AesProviderTest
    {
        [Theory]
        [InlineData(AesKeySize.AES128Bits)]
        [InlineData(AesKeySize.AES192Bits)]
        [InlineData(AesKeySize.AES256Bits)]
        public void EncryptDecryptByteArrayTest(AesKeySize keySize)
        {
            byte[] input = DataHelper.RandomBytes(20);
            AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(keySize);
            var provider = new AesProvider(encryptionKeyInfo.Key);

            byte[] encryptedData = provider.Encrypt(input, b => b, StandardConverters.StreamToBytes);
            Assert.NotNull(encryptedData);

            byte[] decryptedData = provider.Decrypt(encryptedData, b => b, StandardConverters.StreamToBytes);
            Assert.NotNull(decryptedData);

            Assert.Equal(input, decryptedData);
        }

        [Theory]
        [InlineData(AesKeySize.AES128Bits)]
        [InlineData(AesKeySize.AES192Bits)]
        [InlineData(AesKeySize.AES256Bits)]
        public void EncryptDecryptStringTest(AesKeySize keySize)
        {
            string input = DataHelper.RandomString(20);
            AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(keySize);
            var provider = new AesProvider(encryptionKeyInfo.Key);

            string encryptedData = provider.Encrypt(input, Encoding.UTF8.GetBytes, StandardConverters.StreamToBase64String);
            Assert.NotNull(encryptedData);

            string decryptedData = provider.Decrypt(encryptedData, Convert.FromBase64String, StandardConverters.StreamToString);
            Assert.NotNull(decryptedData);

            Assert.Equal(input, decryptedData);
        }

        [Theory]
        [InlineData(AesKeySize.AES128Bits)]
        [InlineData(AesKeySize.AES192Bits)]
        [InlineData(AesKeySize.AES256Bits)]
        public void EncryptDecryptSecureStringTest(AesKeySize keySize)
        {
            SecureString input = DataHelper.RandomSecureString(20);
            AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(keySize);
            var provider = new AesProvider(encryptionKeyInfo.Key);

            string encryptedData = provider.Encrypt(input, Encoding.UTF8.GetBytes, StandardConverters.StreamToBase64String);
            Assert.NotNull(encryptedData);

            SecureString decryptedData = provider.Decrypt(encryptedData, Convert.FromBase64String, StandardConverters.StreamToSecureString);
            Assert.NotNull(decryptedData);

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] decryptedBytes = Encoding.UTF8.GetBytes(decryptedData);

            Assert.Equal(inputBytes, decryptedBytes);
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
        public void CreateDataContextWithoutProvider()
        {
            using var contextFactory = new DatabaseContextFactory();
            using var context = contextFactory.CreateContext<SimpleEncryptedDatabaseContext>();
            Assert.NotNull(context);
        }

        [Fact]
        public void EncryptUsingAes128Provider()
        {
            ExecuteAesEncryptionTest<Aes128EncryptedDatabaseContext>(AesKeySize.AES128Bits);
        }

        [Fact]
        public void EncryptUsingAes192Provider()
        {
            ExecuteAesEncryptionTest<Aes192EncryptedDatabaseContext>(AesKeySize.AES192Bits);
        }

        [Fact]
        public void EncryptUsingAes256Provider()
        {
            ExecuteAesEncryptionTest<Aes256EncryptedDatabaseContext>(AesKeySize.AES256Bits);
        }

        private static void ExecuteAesEncryptionTest<TContext>(AesKeySize aesKeyType) where TContext : DatabaseContext
        {
            AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(aesKeyType);
            var provider = new AesProvider(encryptionKeyInfo.Key, CipherMode.CBC, PaddingMode.Zeros);
            var author = new AuthorEntity("John", "Doe", 42)
            {
                Password = DataHelper.RandomSecureString(10),
                Books = new List<BookEntity>
                {
                    new("Lorem Ipsum", 300),
                    new("Dolor sit amet", 390)
                }
            };

            using var contextFactory = new DatabaseContextFactory();

            // Save data to an encrypted database context
            using (var dbContext = contextFactory.CreateContext<TContext>(provider))
            {
                dbContext.Authors.Add(author);
                dbContext.SaveChanges();
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

        public class SimpleEncryptedDatabaseContext : DatabaseContext
        {
            public SimpleEncryptedDatabaseContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
                : base(options, encryptionProvider) { }
        }
    }
}
