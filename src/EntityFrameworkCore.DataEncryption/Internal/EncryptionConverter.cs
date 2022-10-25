using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security;
using System.Text;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal;

/// <summary>
/// Defines the internal encryption converter for string values.
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <typeparam name="TProvider"></typeparam>
internal sealed class EncryptionConverter<TModel, TProvider> : ValueConverter<TModel, TProvider>, IEncryptionValueConverter
{
    public IEncryptionProvider EncryptionProvider { get; }

    /// <summary>
    /// Creates a new <see cref="EncryptionConverter{TModel,TProvider}"/> instance.
    /// </summary>
    public EncryptionConverter(IEncryptionProvider encryptionProvider, StorageFormat storageFormat, ConverterMappingHints mappingHints = null)
        : base(
            x => Encrypt<TModel, TProvider>(x, encryptionProvider, storageFormat),
            x => Decrypt<TModel, TProvider>(x, encryptionProvider, storageFormat), 
            mappingHints)
    {
        EncryptionProvider = encryptionProvider;
    }

    private static TOutput Encrypt<TInput, TOutput>(TInput input, IEncryptionProvider encryptionProvider, StorageFormat storageFormat)
    {
        byte[] inputData = input switch
        {
            string => Encoding.UTF8.GetBytes(input.ToString()),
            byte[] => input as byte[],
            SecureString => null,
            _ => null,
        };

        byte[] encryptedRawBytes = encryptionProvider.Encrypt(inputData);

        object encryptedData = storageFormat switch
        {
            StorageFormat.Default or StorageFormat.Base64 => Convert.ToBase64String(encryptedRawBytes),
            _ => encryptedRawBytes
        };

        return (TOutput)Convert.ChangeType(encryptedData, typeof(TOutput));
    }

    private static TModel Decrypt<TInput, TOupout>(TProvider input, IEncryptionProvider encryptionProvider, StorageFormat storageFormat)
    {
        Type destinationType = typeof(TModel);
        byte[] inputData = storageFormat switch
        {
            StorageFormat.Default or StorageFormat.Base64 => Convert.FromBase64String(input.ToString()),
            _ => input as byte[]
        };

        byte[] decryptedRawBytes = encryptionProvider.Decrypt(inputData);

        object decryptedData = null;

        if (destinationType == typeof(string))
        {
            decryptedData = Encoding.UTF8.GetString(decryptedRawBytes).Trim('\0');
        }
        else if (destinationType == typeof(byte[]))
        {
            decryptedData = decryptedRawBytes;
        }
        else if (destinationType == typeof(SecureString))
        {
            // TODO
        }

        return (TModel)Convert.ChangeType(decryptedData, typeof(TModel));
    }
}
