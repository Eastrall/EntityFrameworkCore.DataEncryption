using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Migration
{
    [DebuggerDisplay("{EntityType.Name}")]
    internal class EncryptedEntity
    {
        public Type EntityType { get; }

        public IEnumerable<PropertyInfo> EncryptedProperties { get; }

        public EncryptedEntity(Type entityType, IEnumerable<PropertyInfo> encryptedProperties)
        {
            EntityType = entityType;
            EncryptedProperties = encryptedProperties;
        }
    }
}
