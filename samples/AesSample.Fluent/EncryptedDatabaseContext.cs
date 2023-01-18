using Microsoft.EntityFrameworkCore;

namespace AesSample.Fluent;

public class EncryptedDatabaseContext : DatabaseContext
{
    public EncryptedDatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options, null)
    {
    }
}