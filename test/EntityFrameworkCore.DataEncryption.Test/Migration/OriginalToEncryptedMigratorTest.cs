using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.DataEncryption.Migration;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Migration
{
    public class OriginalToEncryptedMigratorTest : MigratorBaseTest
    {
        [Fact]
        public async Task MigrateOriginalToEncryptedTest()
        {
            var aesKeys = AesProvider.GenerateKey(AesKeySize.AES256Bits);
            var destinationProvider = new AesProvider(aesKeys.Key);
            var provider = new MigrationEncryptionProvider(null, destinationProvider);
            await Execute(provider);
        }
    }
}