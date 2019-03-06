using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Encryption
{
    public static class ModelBuilderExtensions
    {
        public static void UseEncryption(this ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (IMutableProperty property in entityType.GetProperties())
                {
                    object[] attributes = property.PropertyInfo.GetCustomAttributes(typeof(EncryptedAttribute), false);
                    
                    // TODO: set value converter here
                }
            }
        }
    }
}
