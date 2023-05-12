using Bogus;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test;

public class PropertyBuilderExtensionsTest
{
    private static readonly Faker _faker = new();

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

        string name = _faker.Name.FullName();
        byte[] bytes = _faker.Random.Bytes(_faker.Random.Int(10, 30));

        UserEntity user = new()
        {
            Name = name,
            NameAsBytes = name,
            ExtraData = bytes,
            ExtraDataAsBytes = bytes,
            EmptyString = ""
        };

        using var contextFactory = new DatabaseContextFactory();
        using (var context = contextFactory.CreateContext<FluentDbContext>(provider))
        {
            Assert.NotNull(context);

            IEntityType entityType = context.GetUserEntityType();
            Assert.NotNull(entityType);

            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.Name)), true, StorageFormat.Default);
            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.NameAsBytes)), true, StorageFormat.Binary);
            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.ExtraData)), true, StorageFormat.Base64);
            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.ExtraDataAsBytes)), true, StorageFormat.Binary);
            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.Id)), false, StorageFormat.Default);
            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.EmptyString)), true, StorageFormat.Base64);

            context.Users.Add(user);
            context.SaveChanges();
        }

        using (var context = contextFactory.CreateContext<FluentDbContext>(provider))
        {
            UserEntity u = context.Users.First();

            Assert.NotNull(u);
            Assert.Equal(name, u.Name);
            Assert.Equal(name, u.NameAsBytes);
            Assert.Equal(bytes, u.ExtraData);
            Assert.Equal(bytes, u.ExtraDataAsBytes);
            Assert.Null(u.EmptyString);
        }
    }

    private static void AssertPropertyAnnotations(IProperty property, bool shouldBeEncrypted, StorageFormat expectedStorageFormat)
    {
        Assert.NotNull(property);

        IAnnotation encryptedAnnotation = property.FindAnnotation(PropertyAnnotations.IsEncrypted);

        if (shouldBeEncrypted)
        {
            Assert.NotNull(encryptedAnnotation);
            Assert.True((bool)encryptedAnnotation.Value);

            IAnnotation formatAnnotation = property.FindAnnotation(PropertyAnnotations.StorageFormat);
            Assert.NotNull(formatAnnotation);
            Assert.Equal(expectedStorageFormat, formatAnnotation.Value);
        }
        else
        {
            Assert.Null(encryptedAnnotation);
            Assert.Null(property.FindAnnotation(PropertyAnnotations.StorageFormat));
        }
    }

    private class UserEntity
    {
        public int Id { get; set; }

        // Encrypted as default (Base64)
        public string Name { get; set; }

        // Encrypted as raw byte array.
        public string NameAsBytes { get; set; }

        // Encrypted as Base64 string
        public byte[] ExtraData { get; set; }

        // Encrypted as raw byte array.
        public byte[] ExtraDataAsBytes { get; set; }

        // Encrypt as Base64 string, but will be empty.
        public string EmptyString { get; set; }
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
            userEntityBuilder.Property(x => x.NameAsBytes).IsRequired().HasColumnType("BLOB").IsEncrypted(StorageFormat.Binary);
            userEntityBuilder.Property(x => x.ExtraData).IsRequired().HasColumnType("TEXT").IsEncrypted(StorageFormat.Base64);
            userEntityBuilder.Property(x => x.ExtraDataAsBytes).IsRequired().HasColumnType("BLOB").IsEncrypted(StorageFormat.Binary);
            userEntityBuilder.Property(x => x.EmptyString).IsRequired(false).HasColumnType("TEXT").IsEncrypted(StorageFormat.Base64);

            modelBuilder.UseEncryption(_encryptionProvider);
        }

        public IEntityType GetUserEntityType() => Model.FindEntityType(typeof(UserEntity));
    }
}
