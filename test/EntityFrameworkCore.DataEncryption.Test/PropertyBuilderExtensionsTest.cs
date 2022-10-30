using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test;

public class PropertyBuilderExtensionsTest
{
    [Fact]
    public void PropertyBuilderShouldNeverBeNullTest()
    {
        Assert.Throws<ArgumentNullException>(() => PropertyBuilderExtensions.IsEncrypted<object>(null));
    }

    [Fact]
    public void PropertyShouldHaveEncryptionAnnotationsTest()
    {
        AesKeyInfo encryptionKeyInfo = AesProvider.GenerateKey(AesKeySize.AES256Bits);
        var provider = new AesProvider(encryptionKeyInfo.Key, encryptionKeyInfo.IV);

        using var contextFactory = new DatabaseContextFactory();
        using var context = contextFactory.CreateContext<FluentDbContext>(provider);
        Assert.NotNull(context);

        IEntityType entityType = context.GetUserEntityType();
        Assert.NotNull(entityType);

        IProperty property = entityType.GetProperty("Name");
        Assert.NotNull(property);

        IAnnotation encryptedAnnotation = property.FindAnnotation(PropertyAnnotations.IsEncrypted);
        IAnnotation formatAnnotation = property.FindAnnotation(PropertyAnnotations.StorageFormat);
        Assert.NotNull(encryptedAnnotation);
        Assert.True((bool)encryptedAnnotation.Value);
        Assert.NotNull(formatAnnotation);
        Assert.Equal(StorageFormat.Default, formatAnnotation.Value);
    }

    private class UserEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private class FluentDbContext : DbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public DbSet<UserEntity> Users { get; set; }

        public FluentDbContext(DbContextOptions options)
        : base(options)
        { }

        public FluentDbContext(DbContextOptions options, IEncryptionProvider encryptionProvider)
            : base(options)
        {
            _encryptionProvider = encryptionProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var userEntityBuilder = modelBuilder.Entity<UserEntity>();

            userEntityBuilder.HasKey(x => x.Id);
            userEntityBuilder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            userEntityBuilder.Property(x => x.Name).IsRequired().IsEncrypted();

            modelBuilder.UseEncryption(_encryptionProvider);
        }

        public IEntityType GetUserEntityType() => Model.FindEntityType(typeof(UserEntity));
    }
}
