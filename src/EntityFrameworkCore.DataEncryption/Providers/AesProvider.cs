using System.IO;
using System.Security.Cryptography;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Providers;

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
    /// <param name="initializationVector">AES Initialization Vector used for the symmetric encryption.</param>
    /// <param name="mode">Mode for operation used in the symmetric encryption.</param>
    /// <param name="padding">Padding mode used in the symmetric encryption.</param>
    public AesProvider(byte[] key, byte[] initializationVector, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
    {
        _key = key;
        _iv = initializationVector;
        _mode = mode;
        _padding = padding;
    }

    /// <inheritdoc />
    public byte[] Encrypt(byte[] input)
    {
        if (input is null || input.Length == 0)
        {
            return null;
        }

        using var aes = CreateCryptographyProvider(_key, _mode, _padding);
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
        crypto.Write(input, 0, input.Length);
        crypto.FlushFinalBlock();

        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream.ToArray();
    }

    /// <inheritdoc />
    public byte[] Decrypt(byte[] input)
    {
        if (input is null || input.Length == 0)
        {
            return null;
        }

        using var memoryStream = new MemoryStream(input);

        byte[] initializationVector = _iv;
        if (initializationVector is null)
        {
            initializationVector = new byte[InitializationVectorSize];
            memoryStream.Read(initializationVector, 0, initializationVector.Length);
        }

        using var aes = CreateCryptographyProvider(_key, _mode, _padding);
        using var transform = aes.CreateDecryptor(_key, initializationVector);

        using var outputStream = new MemoryStream();
        using var crypto = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);

        crypto.CopyTo(outputStream);

        return outputStream.ToArray();
    }

    /// <summary>
    /// Generates an AES cryptography provider.
    /// </summary>
    /// <returns></returns>
    private static Aes CreateCryptographyProvider(byte[] key, CipherMode mode, PaddingMode padding)
    {
        var aes = Aes.Create();

        aes.BlockSize = AesBlockSize;
        aes.Mode = mode;
        aes.Padding = padding;
        aes.Key = key;
        aes.KeySize = key.Length * 8;

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
        var aes = Aes.Create();

        aes.KeySize = (int)keySize;
        aes.BlockSize = AesBlockSize;

        aes.GenerateKey();
        aes.GenerateIV();

        return new AesKeyInfo(aes.Key, aes.IV);
    }
}
