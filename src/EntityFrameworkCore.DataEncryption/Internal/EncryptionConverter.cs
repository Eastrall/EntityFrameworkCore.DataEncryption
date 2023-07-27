using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
    /// <param name="modelSerialization"></param>
    /// <param name="providerSerialization"></param>
    /// <param name="mappingHints">Mapping hints.</param>
    public EncryptionConverter(
        IEncryptionProvider encryptionProvider,
        ISerializationProvider modelSerialization,
        ISerializationProvider providerSerialization,
        ConverterMappingHints mappingHints = null)
        : base(
            x => Encrypt<TModel, TProvider>(x, encryptionProvider, modelSerialization, providerSerialization),
            x => Decrypt<TModel, TProvider>(x, encryptionProvider, modelSerialization, providerSerialization),
            mappingHints)
    {
    }

    private static TOutput Encrypt<TInput, TOutput>(TInput input, IEncryptionProvider encryptionProvider, ISerializationProvider modelSerialization, ISerializationProvider providerSerialization)
    {
        byte[] inputData = modelSerialization.Serialize(input);
        byte[] encryptedRawBytes = encryptionProvider.Encrypt(inputData);
        if (encryptedRawBytes is null || encryptedRawBytes.Length == 0)
            return default;
        return providerSerialization.Deserialize<TOutput>(encryptedRawBytes);
    }

    private static TInput Decrypt<TInput, TOutput>(TOutput input, IEncryptionProvider encryptionProvider, ISerializationProvider modelSerialization, ISerializationProvider providerSerialization)
    {
        byte[] inputData = providerSerialization.Serialize(input);
        byte[] decryptedRawBytes = encryptionProvider.Decrypt(inputData);
        if (decryptedRawBytes is null || decryptedRawBytes.Length == 0)
            return default;
        return modelSerialization.Deserialize<TInput>(decryptedRawBytes);
    }
}
