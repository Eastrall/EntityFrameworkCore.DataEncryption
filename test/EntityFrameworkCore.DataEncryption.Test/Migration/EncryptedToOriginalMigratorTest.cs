using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.DataEncryption.Migration;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Migration
{
    public class EncryptedToOriginalMigratorTest : MigratorBaseTest
    {
        [Fact]
        public async Task MigrateEncryptedToOriginalTest()
        {
            var aesKeys = AesProvider.GenerateKey(AesKeySize.AES256Bits);
            var sourceProvider = new AesProvider(aesKeys.Key);
            var provider = new MigrationEncryptionProvider(sourceProvider, null);
            await Execute(provider);
        }
    }
}