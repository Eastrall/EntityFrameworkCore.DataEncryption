using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption;

namespace AesSample
{
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
            modelBuilder.UseEncryption(_encryptionProvider);

            base.OnModelCreating(modelBuilder);
        }
    }
}
