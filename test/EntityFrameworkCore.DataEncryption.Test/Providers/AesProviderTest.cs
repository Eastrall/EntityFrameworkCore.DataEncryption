using Bogus;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Providers;

public class AesProviderTest
{
    private static readonly Faker _faker = new();

    [Fact]
    public void CreateAesProviderWithoutKeyTest()
    {
        Assert.Throws<ArgumentNullException>(() => new AesProvider(null, null));
    }

    [Fact]
    public void CreateAesProviderWithoutInitializationVectorTest()
    {
        Assert.Throws<ArgumentNullException>(() => new AesProvider(Array.Empty<byte>(), null));
    }

    [Fact]
    public void EncryptNullOrEmptyDataTest()
    {
        AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(AesKeySize.AES256Bits);
        var provider = new AesProvider(encryptionKeyInfo.Key, encryptionKeyInfo.IV);

        Assert.Null(provider.Encrypt(null));
        Assert.Null(provider.Encrypt(Array.Empty<byte>()));
    }

    [Fact]
    public void DecryptNullOrEmptyDataTest()
    {
        AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(AesKeySize.AES256Bits);
        var provider = new AesProvider(encryptionKeyInfo.Key, encryptionKeyInfo.IV);

        Assert.Null(provider.Decrypt(null));
        Assert.Null(provider.Decrypt(Array.Empty<byte>()));
    }

    [Theory]
    [InlineData(AesKeySize.AES128Bits)]
    [InlineData(AesKeySize.AES192Bits)]
    [InlineData(AesKeySize.AES256Bits)]
    public void EncryptDecryptByteArrayTest(AesKeySize keySize)
    {
        byte[] input = _faker.Random.Bytes(_faker.Random.Int(10, 30));
        AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(keySize);
        var provider = new AesProvider(encryptionKeyInfo.Key, encryptionKeyInfo.IV);

        byte[] encryptedData = provider.Encrypt(input);
        Assert.NotNull(encryptedData);

        byte[] decryptedData = provider.Decrypt(encryptedData);
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
        var provider = new AesProvider(encryptionKeyInfo.Key, encryptionKeyInfo.IV);
        var author = new AuthorEntity("John", "Doe", 42)
        {
            Books = new List<BookEntity>
            {
                new(name: _faker.Lorem.Sentence(2),
                    numberOfPages: _faker.Random.Int(100, 300),
                    content: _faker.Random.Bytes(_faker.Random.Int(5, 10)), 
                    summary: _faker.Random.Bytes(_faker.Random.Int(5, 10))),
                new(name: _faker.Lorem.Sentence(2),
                    numberOfPages: _faker.Random.Int(100, 300),
                    content: _faker.Random.Bytes(_faker.Random.Int(5, 10)),
                    summary: _faker.Random.Bytes(_faker.Random.Int(5, 10))),
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
            AuthorEntity authorFromDb = dbContext.Authors.Include(x => x.Books).FirstOrDefault();

            Assert.NotNull(authorFromDb);
            Assert.Equal(author.FirstName, authorFromDb.FirstName);
            Assert.Equal(author.LastName, authorFromDb.LastName);
            Assert.NotNull(authorFromDb.Books);
            Assert.NotEmpty(authorFromDb.Books);
            Assert.Equal(2, authorFromDb.Books.Count);

            foreach (var book in authorFromDb.Books)
            {
                BookEntity originalBook = author.Books.FirstOrDefault(x => x.Id == book.Id);

                Assert.NotNull(originalBook);
                Assert.Equal(originalBook.Name, book.Name);
                Assert.Equal(originalBook.Content, book.Content);
                Assert.Equal(originalBook.Summary, book.Summary);
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

    public class SimpleEncryptedDatabaseContext : DatabaseContext
    {
        public SimpleEncryptedDatabaseContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
            : base(options, encryptionProvider) { }
    }
}
