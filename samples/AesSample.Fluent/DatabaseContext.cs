using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption;
using System.ComponentModel.DataAnnotations;

namespace AesSample.Fluent;

public class DatabaseContext : DbContext
{
    private readonly IEncryptionProvider _encryptionProvider;

    public DbSet<UserEntity> Users { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options, IEncryptionProvider encryptionProvider)
        : base(options)
    {
        _encryptionProvider = encryptionProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var userEntityBuilder = modelBuilder.Entity<UserEntity>();

        userEntityBuilder.HasKey(x => x.Id);
        userEntityBuilder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
        userEntityBuilder.Property(x => x.FirstName).IsRequired();
        userEntityBuilder.Property(x => x.LastName).IsRequired();
        userEntityBuilder.Property(x => x.Email).IsRequired().IsEncrypted();
        userEntityBuilder.Property(x => x.EncryptedData).IsRequired().IsEncrypted();
        userEntityBuilder.Property(x => x.EncryptedDataAsString).IsRequired().HasColumnType("TEXT").IsEncrypted(StorageFormat.Base64);

        modelBuilder.UseEncryption(_encryptionProvider);

        base.OnModelCreating(modelBuilder);
    }
}
