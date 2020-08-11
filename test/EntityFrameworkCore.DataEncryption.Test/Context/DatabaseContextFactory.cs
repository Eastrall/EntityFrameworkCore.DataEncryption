using Microsoft.Data.Sqlite;
using System;
using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Context
{
    /// <summary>
    /// Database context factory used to create entity framework new <see cref="DbContext"/>.
    /// </summary>
    public sealed class DatabaseContextFactory : IDisposable
    {
        private const string DatabaseConnectionString = "DataSource=:memory:";
        private readonly DbConnection _connection;

        /// <summary>
        /// Creates a new <see cref="DatabaseContextFactory"/> instance.
        /// </summary>
        public DatabaseContextFactory()
        {
            _connection = new SqliteConnection(DatabaseConnectionString);
            _connection.Open();

            using (var dbContext = new DatabaseContext(CreateOptions<DatabaseContext>()))
                dbContext.Database.EnsureCreated();
        }

        /// <summary>
        /// Creates a new in memory database context.
        /// </summary>
        /// <typeparam name="TContext">Context</typeparam>
        /// <param name="provider">Encryption provider</param>
        /// <returns></returns>
        public TContext CreateContext<TContext>(IEncryptionProvider provider = null) where TContext : DbContext
        {
            if (provider == null)
                return Activator.CreateInstance(typeof(TContext), CreateOptions<TContext>()) as TContext;

            return Activator.CreateInstance(typeof(TContext), CreateOptions<TContext>(), provider) as TContext;
        }

        /// <summary>
        /// Creates a new <see cref="DbContextOptions"/> instance using SQLite.
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <returns></returns>
        private DbContextOptions<TContext> CreateOptions<TContext>() where TContext : DbContext 
            => new DbContextOptionsBuilder<TContext>().UseSqlite(_connection).Options;

        /// <summary>
        /// Dispose the SQLite in memory connection.
        /// </summary>
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
            }
        }
    }
}
