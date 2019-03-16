using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Encryption.Providers
{
    /// <summary>
    /// Implements the Advanced Encryption Standard (AES) symmetric algorithm.
    /// </summary>
    public class AesProvider : IEncryptionProvider
    {
        private const int AesBlockSize = 128;
        private readonly byte[] _key;
        private readonly byte[] _initializationVector;
        private readonly CipherMode _mode;
        private readonly PaddingMode _padding;

        /// <summary>
        /// Creates a new <see cref="AesProvider"/> instance used to perform symetric encryption and decryption on strings.
        /// </summary>
        /// <param name="key">AES key used for the symetric encryption.</param>
        /// <param name="initializationVector">AES Initialization Vector used for the symetric encryption.</param>
        /// <param name="mode">Mode for operation used in the symetric encryption.</param>
        /// <param name="padding">Padding mode used in the symetric encryption.</param>
        public AesProvider(byte[] key, byte[] initializationVector, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
        {
            this._key = key;
            this._initializationVector = initializationVector;
            this._mode = mode;
            this._padding = padding;
        }

        /// <summary>
        /// Encrypt a string using the AES algorithm.
        /// </summary>
        /// <param name="dataToEncrypt"></param>
        /// <returns></returns>
        public string Encrypt(string dataToEncrypt)
        {
            byte[] input = Encoding.UTF8.GetBytes(dataToEncrypt);
            byte[] encrypted = null;

            using (var aes = this.CreateNewAesEncryptor())
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var crypto = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        crypto.Write(input, 0, input.Length);

                    encrypted = memoryStream.ToArray();
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypt a string using the AES algorithm.
        /// </summary>
        /// <param name="dataToDecrypt"></param>
        /// <returns></returns>
        public string Decrypt(string dataToDecrypt)
        {
            byte[] input = Convert.FromBase64String(dataToDecrypt);
            string decrypted = string.Empty;

            using (var aes = this.CreateNewAesEncryptor())
            {
                using (var memoryStream = new MemoryStream(input))
                using (var crypto = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var sr = new StreamReader(crypto))
                    decrypted = sr.ReadToEnd().Trim('\0');
            }

            return decrypted;
        }

        /// <summary>
        /// Creates a new <see cref="Aes"/> instance with the current configuration.
        /// </summary>
        /// <returns></returns>
        private Aes CreateNewAesEncryptor()
        {
            var aes = Aes.Create();

            aes.Mode = this._mode;
            aes.Padding = this._padding;
            aes.Key = this._key;
            aes.IV = this._initializationVector;

            return aes;
        }

        /// <summary>
        /// Generates an AES key.
        /// </summary>
        /// <remarks>
        /// The key size of the Aes encryption must be 128, 192 or 256 bits. 
        /// Please check https://blogs.msdn.microsoft.com/shawnfa/2006/10/09/the-differences-between-rijndael-and-aes/ for more informations.
        /// </remarks>
        /// <param name="keySize">AES Key size</param>
        /// <returns></returns>
        public static AesKeyInfo GenerateKey(AesKeySize keySize)
        {
            var crypto = new AesCryptoServiceProvider
            {
                KeySize = (int)keySize,
                BlockSize = AesBlockSize
            };

            crypto.GenerateKey();
            crypto.GenerateIV();

            return new AesKeyInfo(crypto.Key, crypto.IV);
        }
    }
}
