namespace Microsoft.EntityFrameworkCore.DataEncryption
{
    /// <summary>
    /// Provides a mechanism for implementing a custom encryption provider.
    /// </summary>
    public interface IEncryptionProvider
    {
        /// <summary>
        /// Encrypts a string.
        /// </summary>
        /// <param name="dataToEncrypt">Input data as a string to encrypt.</param>
        /// <returns>Encrypted data as a string.</returns>
        string Encrypt(string dataToEncrypt);

        /// <summary>
        /// Decrypts a string.
        /// </summary>
        /// <param name="dataToDecrypt">Encrypted data as a string to decrypt.</param>
        /// <returns>Decrypted data as a string.</returns>
        string Decrypt(string dataToDecrypt);
    }
}
