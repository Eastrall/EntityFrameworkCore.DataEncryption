namespace Microsoft.EntityFrameworkCore.DataEncryption;

/// <summary>
/// Provides a mechanism to encrypt and decrypt data.
/// </summary>
public interface IEncryptionProvider
{
    /// <summary>
    /// Encrypts the given input byte array.
    /// </summary>
    /// <param name="input">Input to encrypt.</param>
    /// <returns>Encrypted input.</returns>
    byte[] Encrypt(byte[] input);

    /// <summary>
    /// Decrypts the given input byte array.
    /// </summary>
    /// <param name="input">Input to decrypt.</param>
    /// <returns>Decrypted input.</returns>
    byte[] Decrypt(byte[] input);
}
