namespace Microsoft.EntityFrameworkCore.Encryption.Test
{
    public sealed class DatabaseContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption();
        }
    }
}
