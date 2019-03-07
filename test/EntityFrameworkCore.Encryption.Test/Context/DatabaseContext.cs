namespace Microsoft.EntityFrameworkCore.Encryption.Test.Context
{
    public sealed class DatabaseContext : DbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public DbSet<AuthorEntity> Authors { get; set; }

        public DbSet<BookEntity> Books { get; set; }

        public DatabaseContext()
        { }

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IEncryptionProvider encryptionProvider = null)
            : base(options)
        {
            this._encryptionProvider = encryptionProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(this._encryptionProvider);
        }
    }
}
