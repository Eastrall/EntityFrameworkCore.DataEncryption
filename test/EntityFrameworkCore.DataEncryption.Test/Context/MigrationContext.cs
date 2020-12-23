namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Context
{
    public class MigrationContext : DbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public DbSet<AuthorEntity> Authors { get; set; }

        public DbSet<BookEntity> Books { get; set; }

        public MigrationContext(DbContextOptions options)
            : base(options)
        { }

        public MigrationContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
            : base(options)
        {
            _encryptionProvider = encryptionProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (_encryptionProvider is not null)
            {
                modelBuilder.UseEncryption(_encryptionProvider);
            }
        }
    }
}
