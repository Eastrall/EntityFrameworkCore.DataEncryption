using System;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Migration.Internal
{
    internal class DataMigratorBase
    {
        protected DataMigratorBase()
        {
        }

        /// <summary>
        /// Gets the DbSet based on the given entity type.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        protected IQueryable<object> GetDbSet(DbContext context, Type entityType)
        {
            return (IQueryable<object>)context.GetType()
                .GetMethods()
                .FirstOrDefault(x => x.Name.StartsWith("Set"))
                .MakeGenericMethod(entityType)
                .Invoke(context, null);
        }
    }
}
