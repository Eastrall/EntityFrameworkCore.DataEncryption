using System;
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
    private readonly byte[] _iv;
    private readonly CipherMode _mode;
    private readonly PaddingMode _padding;

    /// <summary>
    /// Creates a new <see cref="AesProvider"/> instance used to perform symmetric encryption and decryption on strings.
    /// </summary>
    /// <param name="key">AES key used for the symmetric encryption.</param>
    /// <param name="initializationVector">AES Initialization Vector used for the symmetric encryption.</param>
    /// <param name="mode">Mode for operation used in the symmetric encryption.</param>
    /// <param name="padding">Padding mode used in the symmetric encryption.</param>
    public AesProvider(byte[] key, byte[] initializationVector, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key), "");
        _iv = initializationVector ?? throw new ArgumentNullException(nameof(initializationVector), "");
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

        using Aes aes = CreateCryptographyProvider(_key, _iv, _mode, _padding);
        using ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, transform, CryptoStreamMode.Write);

        cryptoStream.Write(input, 0, input.Length);
        cryptoStream.FlushFinalBlock();
        memoryStream.Seek(0L, SeekOrigin.Begin);

        return StreamToBytes(memoryStream);
    }

    /// <inheritdoc />
    public byte[] Decrypt(byte[] input)
    {
        if (input is null || input.Length == 0)
        {
            return null;
        }

        using Aes aes = CreateCryptographyProvider(_key, _iv, _mode, _padding);
        using ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream memoryStream = new(input);
        using CryptoStream cryptoStream = new(memoryStream, transform, CryptoStreamMode.Read);

        return StreamToBytes(cryptoStream);
    }

    /// <summary>
    /// Converts a <see cref="Stream"/> into a byte array.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <returns>The stream's content as a byte array.</returns>
    internal static byte[] StreamToBytes(Stream stream)
    {
        if (stream is MemoryStream ms)
        {
            return ms.ToArray();
        }

        using var output = new MemoryStream();
        stream.CopyTo(output);
        return output.ToArray();
    }

    /// <summary>
    /// Generates an AES cryptography provider.
    /// </summary>
    /// <returns></returns>
    private static Aes CreateCryptographyProvider(byte[] key, byte[] iv, CipherMode mode, PaddingMode padding)
    {
        var aes = Aes.Create();

        aes.Mode = mode;
        aes.KeySize = key.Length * 8;
        aes.BlockSize = AesBlockSize;
        aes.FeedbackSize = AesBlockSize;
        aes.Padding = padding;
        aes.Key = key;
        aes.IV = iv;

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
