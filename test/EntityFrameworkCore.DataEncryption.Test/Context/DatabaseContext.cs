using Microsoft.EntityFrameworkCore.Encryption.Test.Context;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Context
{
    public class DatabaseContext : DbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public DbSet<PublisherEntity> Publishers { get; set; }

        public DbSet<AuthorEntity> Authors { get; set; }

        public DbSet<BookEntity> Books { get; set; }

        public DatabaseContext(DbContextOptions options)
            : base(options)
        { }

        public DatabaseContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
            : base(options)
        {
            _encryptionProvider = encryptionProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(_encryptionProvider);
            modelBuilder.ApplyConfiguration(new PublisherEntityConfiguration(_encryptionProvider));
        }
    }
}