using System;
using System.IO;

namespace Microsoft.EntityFrameworkCore.DataEncryption
{
    /// <summary>
    /// Provides a mechanism for implementing a custom encryption provider.
    /// </summary>
    public interface IEncryptionProvider
    {
        /// <summary>
        /// Encrypts a value.
        /// </summary>
        /// <typeparam name="TStore">
        /// The type of data stored in the database.
        /// </typeparam>
        /// <typeparam name="TModel">
        /// The type of value stored in the model.
        /// </typeparam>
        /// <param name="dataToEncrypt">
        /// Input data to encrypt.
        /// </param>
        /// <param name="converter">
        /// Function which converts the model value to a byte array.
        /// </param>
        /// <param name="encoder">
        /// Function which encodes the value for storing the the database.
        /// </param>
        /// <returns>
        /// Encrypted data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="converter"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="encoder"/> is <see langword="null"/>.</para>
        /// </exception>
        TStore Encrypt<TStore, TModel>(TModel dataToEncrypt, Func<TModel, byte[]> converter, Func<Stream, TStore> encoder);

        /// <summary>
        /// Decrypts a value.
        /// </summary>
        /// <typeparam name="TStore">
        /// The type of data stored in the database.
        /// </typeparam>
        /// <typeparam name="TModel">
        /// The type of value stored in the model.
        /// </typeparam>
        /// <param name="dataToDecrypt">
        /// Encrypted data to decrypt.
        /// </param>
        /// <param name="decoder">
        /// Function which converts the stored data to a byte array.
        /// </param>
        /// <param name="converter">
        /// Function which converts the decrypted <see cref="Stream"/> to the return value.
        /// </param>
        /// <returns>
        /// Decrypted data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="decoder"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="converter"/> is <see langword="null"/>.</para>
        /// </exception>
        TModel Decrypt<TStore, TModel>(TStore dataToDecrypt, Func<TStore, byte[]> decoder, Func<Stream, TModel> converter);
    }
}
