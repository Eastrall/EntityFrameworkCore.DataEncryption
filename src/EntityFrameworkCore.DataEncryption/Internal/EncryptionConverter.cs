using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal;

/// <summary>
/// Defines the internal encryption converter for string values.
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <typeparam name="TProvider"></typeparam>
internal sealed class EncryptionConverter<TModel, TProvider> : ValueConverter<TModel, TProvider>
{
    /// <summary>
    /// Creates a new <see cref="EncryptionConverter{TModel,TProvider}"/> instance.
    /// </summary>
    /// <param name="encryptionProvider">Encryption provider to use.</param>
    /// <param name="storageFormat">Encryption storage format.</param>
    /// <param name="mappingHints">Mapping hints.</param>
    public EncryptionConverter(IEncryptionProvider encryptionProvider, StorageFormat storageFormat, ConverterMappingHints mappingHints = null)
        : base(
            x => Encrypt<TModel, TProvider>(x, encryptionProvider, storageFormat),
            x => Decrypt<TModel, TProvider>(x, encryptionProvider, storageFormat),
            mappingHints)
    {
    }

    private static TOutput Encrypt<TInput, TOutput>(TInput input, IEncryptionProvider encryptionProvider, StorageFormat storageFormat)
    {
        byte[] inputData = input switch
        {
            string => !string.IsNullOrEmpty(input.ToString()) ? Encoding.UTF8.GetBytes(input.ToString()) : null,
            byte[] => input as byte[],
            _ => JsonSerializer.SerializeToUtf8Bytes(input),
        };

        byte[] encryptedRawBytes = encryptionProvider.Encrypt(inputData);
        if (encryptedRawBytes is null || encryptedRawBytes.Length == 0)
            return default;

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
        if (decryptedRawBytes is null || decryptedRawBytes.Length == 0)
            return default;
        
        if (destinationType == typeof(string))
        {
            string decryptedData = Encoding.UTF8.GetString(decryptedRawBytes).Trim('\0');
            return (TModel)Convert.ChangeType(decryptedData, typeof(TModel));
        }
        else if (destinationType == typeof(byte[]))
        {
            return (TModel)Convert.ChangeType(decryptedRawBytes, typeof(TModel));
        }

        return JsonSerializer.Deserialize<TModel>(decryptedRawBytes);
    }
}
