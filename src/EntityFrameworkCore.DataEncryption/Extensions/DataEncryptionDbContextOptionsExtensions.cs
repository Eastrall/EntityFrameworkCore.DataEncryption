using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Data.Common;
using System.Security.Cryptography;

namespace Microsoft.EntityFrameworkCore
{
    //
    // Summary:
    //     DataEncryption specific extension methods for Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.
    public static class DataEncryptionDbContextOptionsExtensions
    {
        //
        // Summary:
        //     Configures the context to impliment a Microsoft SQL Server database.
        //
        // Parameters:
        //   optionsBuilder:
        //     The builder being used to configure the context.
        //
        //   key:
        //     AES key used for the symetric encryption.
        //
        //   mode:
        //     Mode for operation used in the symetric encryption.
        //
        //   padding:
        //     Padding mode used in the symetric encryption.
        //
        //   dataEncryptionOptionsAction:
        //     An optional action to allow additional Data Encryption specific configuration.
        //
        // Returns:
        //     The options builder so that further configuration can be chained.
        public static DbContextOptionsBuilder UseAesProvider([byte[] key, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, Action<DataEncryptionDbContextOptionsBuilder> dataEncryptionOptionsAction = null);