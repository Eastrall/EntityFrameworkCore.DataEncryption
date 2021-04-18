using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Migration
{
    /// <summary>
    /// Utilities for migrating encrypted data from one provider to another.
    /// </summary>
    /// <example>
    /// <para>To migrate from v1 to v2 of <see cref="AesProvider"/>:</para>
    /// <code>
    /// var sourceProvider = new AesProvider(key, iv);
    /// var destinationProvider = new AesProvider(key);
    /// var migrationProvider = new MigrationEncryptionProvider(sourceProvider, destinationProvider);
    /// await using var migrationContext = new DatabaseContext(options, migrationProvider);
    /// await migrationContext.MigrateAsync(logger, cancellationToken);
    /// </code>
    /// </example>
    public static class EncryptionMigrator
    {
        private static readonly MethodInfo SetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set));

        private static IQueryable<object> Set(this DbContext context, IEntityType entityType)
        {
            var method = SetMethod.MakeGenericMethod(entityType.ClrType);
            var result = method.Invoke(context, null);
            return (IQueryable<object>)result;
        }

        /// <summary>
        /// Migrates the data for a single property to a new encryption provider.
        /// </summary>
        /// <param name="context">
        /// The <see cref="DbContext"/>.
        /// </param>
        /// <param name="property">
        /// The <see cref="IProperty"/> to migrate.
        /// </param>
        /// <param name="logger">
        /// The <see cref="ILogger"/> to use, if any.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> to use, if any.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="context"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="property"/> is <see langword="null"/>.</para>
        /// </exception>
        public static async Task MigrateAsync(this DbContext context, IProperty property, ILogger logger = default, CancellationToken cancellationToken = default)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (property.GetValueConverter() is not IEncryptionValueConverter converter)
            {
                logger?.LogWarning("Property {Property} on entity type {EntityType} is not using an encryption value converter. ({Converter})",
                    property.Name, property.DeclaringEntityType.Name, property.GetValueConverter());

                return;
            }

            if (converter.EncryptionProvider is not MigrationEncryptionProvider { IsEmpty: false })
            {
                logger?.LogWarning("Property {Property} on entity type {EntityType} is not using a non-empty migration encryption value converter. ({EncryptionProvider})",
                    property.Name, property.DeclaringEntityType.Name, converter.EncryptionProvider);

                return;
            }

            logger?.LogInformation("Loading data for {EntityType} ({Property})...",
                property.DeclaringEntityType.Name, property.Name);

            var set = context.Set(property.DeclaringEntityType);
            var list = await set.ToListAsync(cancellationToken);

            logger?.LogInformation("Migrating data for {EntityType} :: {Property}} ({RecordCount} records)...",
                property.DeclaringEntityType.Name, property.Name, list.Count);

            foreach (var entity in list)
            {
                context.Entry(entity).Property(property.Name).IsModified = true;
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async ValueTask MigrateAsyncCore(DbContext context, IEntityType entityType, ILogger logger = default, CancellationToken cancellationToken = default)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (entityType is null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            var encryptedProperties = entityType.GetProperties()
                .Select(p => (property: p, encryptionProvider: (p.GetValueConverter() as IEncryptionValueConverter)?.EncryptionProvider))
                .Where(p => p.encryptionProvider is MigrationEncryptionProvider { IsEmpty: false })
                .Select(p => p.property)
                .ToList();

            if (encryptedProperties.Count == 0)
            {
                logger?.LogDebug("Entity type {EntityType} has no encrypted properties.", entityType.Name);
                return;
            }

            logger?.LogInformation("Loading data for {EntityType} ({PropertyCount} properties)...", entityType.Name, encryptedProperties.Count);

            var set = context.Set(entityType);
            var list = await set.ToListAsync(cancellationToken);
            logger?.LogInformation("Migrating data for {EntityType} ({RecordCount} records)...", entityType.Name, list.Count);

            foreach (var entity in list)
            {
                var entry = context.Entry(entity);
                foreach (var property in encryptedProperties)
                {
                    entry.Property(property.Name).IsModified = true;
                }
            }
        }

        /// <summary>
        /// Migrates the encrypted data for a single entity type to a new encryption provider.
        /// </summary>
        /// <param name="context">
        /// The <see cref="DbContext"/>.
        /// </param>
        /// <param name="entityType">
        /// The <see cref="IEntityType"/> to migrate.
        /// </param>
        /// <param name="logger">
        /// The <see cref="ILogger"/> to use, if any.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> to use, if any.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="context"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="entityType"/> is <see langword="null"/>.</para>
        /// </exception>
        public static async Task MigrateAsync(this DbContext context, IEntityType entityType, ILogger logger = default, CancellationToken cancellationToken = default)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (entityType is null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            await MigrateAsyncCore(context, entityType, logger, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Migrates the encrypted data for the entire context to a new encryption provider.
        /// </summary>
        /// <param name="context">
        /// The <see cref="DbContext"/>.
        /// </param>
        /// <param name="logger">
        /// The <see cref="ILogger"/> to use, if any.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> to use, if any.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="context"/> is <see langword="null"/>.</para>
        /// </exception>
        public static async Task MigrateAsync(this DbContext context, ILogger logger = default, CancellationToken cancellationToken = default)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var entityType in context.Model.GetEntityTypes())
            {
                await MigrateAsyncCore(context, entityType, logger, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}