using System;
using System.IO;
using System.Security.Cryptography;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Providers
{
    /// <summary>
    /// Implements the Advanced Encryption Standard (AES) symmetric algorithm.
    /// </summary>
    public class AesProvider : IEncryptionProvider
    {
        /// <summary>
        /// AES block size constant.
        /// </summary>
        public const int AesBlockSize = 128;

        /// <summary>
        /// Initialization vector size constant.
        /// </summary>
        public const int InitializationVectorSize = 16;

        private readonly byte[] _key;
        private readonly CipherMode _mode;
        private readonly PaddingMode _padding;
        private readonly byte[] _iv;

        /// <summary>
        /// Creates a new <see cref="AesProvider"/> instance used to perform symmetric encryption and decryption on strings.
        /// </summary>
        /// <param name="key">AES key used for the symmetric encryption.</param>
        /// <param name="mode">Mode for operation used in the symmetric encryption.</param>
        /// <param name="padding">Padding mode used in the symmetric encryption.</param>
        public AesProvider(byte[] key, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
        {
            _key = key;
            _mode = mode;
            _padding = padding;
        }

        /// <summary>
        /// Creates a new <see cref="AesProvider"/> instance used to perform symmetric encryption and decryption on strings.
        /// </summary>
        /// <param name="key">AES key used for the symmetric encryption.</param>
        /// <param name="initializationVector">AES Initialization Vector used for the symmetric encryption.</param>
        /// <param name="mode">Mode for operation used in the symmetric encryption.</param>
        /// <param name="padding">Padding mode used in the symmetric encryption.</param>
        public AesProvider(byte[] key, byte[] initializationVector, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7) : this(key, mode, padding)
        {
            // Re-enabled to allow for a static IV.
            // This reduces security, but allows for encrypted values to be searched using LINQ.
            _iv = initializationVector;
        }

        /// <inheritdoc />
        public TStore Encrypt<TStore, TModel>(TModel dataToEncrypt, Func<TModel, byte[]> converter, Func<Stream, TStore> encoder)
        {
            if (converter is null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            byte[] data = converter(dataToEncrypt);
            if (data is null || data.Length == 0)
            {
                return default;
            }

            using var aes = CreateCryptographyProvider();
            using var memoryStream = new MemoryStream();

            byte[] initializationVector = _iv;
            if (initializationVector is null)
            {
                aes.GenerateIV();
                initializationVector = aes.IV;
                memoryStream.Write(initializationVector, 0, initializationVector.Length);
            }

            using var transform = aes.CreateEncryptor(_key, initializationVector);
            using var crypto = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            crypto.Write(data, 0, data.Length);
            crypto.FlushFinalBlock();

            memoryStream.Seek(0L, SeekOrigin.Begin);
            return encoder(memoryStream);
        }

        /// <inheritdoc />
        public TModel Decrypt<TStore, TModel>(TStore dataToDecrypt, Func<TStore, byte[]> decoder, Func<Stream, TModel> converter)
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (converter is null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            byte[] data = decoder(dataToDecrypt);
            if (data is null || data.Length == 0)
            {
                return default;
            }

            using var memoryStream = new MemoryStream(data);

            byte[] initializationVector = _iv;
            if (initializationVector is null)
            {
                initializationVector = new byte[InitializationVectorSize];
                memoryStream.Read(initializationVector, 0, initializationVector.Length);
            }

            using var aes = CreateCryptographyProvider();
            using var transform = aes.CreateDecryptor(_key, initializationVector);
            using var crypto = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            return converter(crypto);
        }

        /// <summary>
        /// Generates an AES cryptography provider.
        /// </summary>
        /// <returns></returns>
        private AesCryptoServiceProvider CreateCryptographyProvider()
        {
            return new AesCryptoServiceProvider
            {
                BlockSize = AesBlockSize,
                Mode = _mode,
                Padding = _padding,
                Key = _key,
                KeySize = _key.Length * 8
            };
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
