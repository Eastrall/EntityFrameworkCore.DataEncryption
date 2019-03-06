using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test
{
    public sealed class EncryptionTest
    {
        [Fact]
        public void CreateDatabaseContext()
        {
            var dbContext = new DatabaseContext();

            Assert.NotNull(dbContext);

            dbContext.Dispose();
        }


    }
}
