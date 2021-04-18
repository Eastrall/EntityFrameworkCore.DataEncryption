using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.DataEncryption.Migration;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Migration
{
    public class V1ToV2MigratorTest : MigratorBaseTest
    {
        [Fact]
        public async Task MigrateV1ToV2Test()
        {
            var aesKeys = AesProvider.GenerateKey(AesKeySize.AES256Bits);
            var sourceProvider = new AesProvider(aesKeys.Key, aesKeys.IV);
            var destinationProvider = new AesProvider(aesKeys.Key);
            var provider = new MigrationEncryptionProvider(sourceProvider, destinationProvider);
            await Execute(provider);
        }
    }
}