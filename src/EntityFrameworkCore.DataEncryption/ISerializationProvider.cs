namespace Microsoft.EntityFrameworkCore.DataEncryption;

/// <summary>
/// Provides a mechanism to serialize data.
/// </summary>
public interface ISerializationProvider
{
    /// <summary>
    /// Serialize the given input to byte array.
    /// </summary>
    /// <param name="input">Input to serialize.</param>
    /// <returns>Serialized input.</returns>
    byte[] Serialize<TModel>(TModel input);

    /// <summary>
    /// Deserialize the given input byte array to object.
    /// </summary>
    /// <param name="input">Input to deserialize.</param>
    /// <returns>Deserialized input.</returns>
    TModel Deserialize<TModel>(byte[] input);
}
