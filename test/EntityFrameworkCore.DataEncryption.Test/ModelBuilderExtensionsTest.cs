using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test;

public class ModelBuilderExtensionsTest
{
    [Fact]
    public void ModelBuilderShouldNeverBeNullTest()
    {
        Assert.Throws<ArgumentNullException>(() => ModelBuilderExtensions.UseEncryption(null, null));
    }

    [Fact]
    public void EncryptionProviderShouldNeverBeNullTest()
    {
        using var contextFactory = new DatabaseContextFactory();

        Assert.Throws<ArgumentNullException>(() => contextFactory.CreateContext<InvalidPropertyDbContext>(null));
    }

    [Fact]
    public void UseEncryptionWithUnsupportedTypeTest()
    {
        AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(AesKeySize.AES256Bits);
        var provider = new AesProvider(encryptionKeyInfo.Key, encryptionKeyInfo.IV);

        using var contextFactory = new DatabaseContextFactory();

        Assert.Throws<NotImplementedException>(() => contextFactory.CreateContext<InvalidPropertyDbContext>(provider));
    }

    private class InvalidPropertyEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Encrypted]
        public string Name { get; set; }

        [Encrypted]
        public int Age { get; set; }
    }

    private class InvalidPropertyDbContext : DbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public DbSet<InvalidPropertyEntity> InvalidEntities { get; set; }

        public InvalidPropertyDbContext(DbContextOptions options)
        : base(options)
        { }

        public InvalidPropertyDbContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
            : base(options)
        {
            _encryptionProvider = encryptionProvider;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(_encryptionProvider);
        }
    }
}
