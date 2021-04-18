using System;
using System.IO;
using System.Security;
using System.Text;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal
{
    /// <summary>
    /// Utilities for building value converters.
    /// </summary>
    public static class ConverterBuilder
    {
        /// <summary>
        /// Builds a converter for a property with a custom model type.
        /// </summary>
        /// <typeparam name="TModelType">
        /// The model type.
        /// </typeparam>
        /// <param name="encryptionProvider">
        /// The <see cref="IEncryptionProvider"/>, if any.
        /// </param>
        /// <param name="decoder">
        /// The function used to decode the model type to a byte array.
        /// </param>
        /// <param name="encoder">
        /// The function used to encode a byte array to the model type.
        /// </param>
        /// <returns>
        /// An <see cref="ConverterBuilder{TModelType}"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="decoder"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="encoder"/> is <see langword="null"/>.</para>
        /// </exception>
        public static ConverterBuilder<TModelType> From<TModelType>(
            this IEncryptionProvider encryptionProvider,
            Func<TModelType, byte[]> decoder,
            Func<Stream, TModelType> encoder)
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            return new ConverterBuilder<TModelType>(encryptionProvider, decoder, encoder);
        }

        /// <summary>
        /// Builds a converter for a binary property.
        /// </summary>
        /// <param name="encryptionProvider">
        /// The <see cref="IEncryptionProvider"/>, if any.
        /// </param>
        /// <returns>
        /// An <see cref="ConverterBuilder{TModelType}"/> instance.
        /// </returns>
        public static ConverterBuilder<byte[]> FromBinary(this IEncryptionProvider encryptionProvider)
        {
            return new ConverterBuilder<byte[]>(encryptionProvider, b => b, StandardConverters.StreamToBytes);
        }

        /// <summary>
        /// Builds a converter for a string property.
        /// </summary>
        /// <param name="encryptionProvider">
        /// The <see cref="IEncryptionProvider"/>, if any.
        /// </param>
        /// <returns>
        /// An <see cref="ConverterBuilder{TModelType}"/> instance.
        /// </returns>
        public static ConverterBuilder<string> FromString(this IEncryptionProvider encryptionProvider)
        {
            return new ConverterBuilder<string>(encryptionProvider, Encoding.UTF8.GetBytes, StandardConverters.StreamToString);
        }

        /// <summary>
        /// Builds a converter for a <see cref="SecureString"/> property.
        /// </summary>
        /// <param name="encryptionProvider">
        /// The <see cref="IEncryptionProvider"/>, if any.
        /// </param>
        /// <returns>
        /// An <see cref="ConverterBuilder{TModelType}"/> instance.
        /// </returns>
        public static ConverterBuilder<SecureString> FromSecureString(this IEncryptionProvider encryptionProvider)
        {
            return new ConverterBuilder<SecureString>(encryptionProvider, Encoding.UTF8.GetBytes, StandardConverters.StreamToSecureString);
        }

        /// <summary>
        /// Specifies that the property should be stored in the database using a custom format.
        /// </summary>
        /// <typeparam name="TModelType">
        /// The model type.
        /// </typeparam>
        /// <typeparam name="TStoreType">
        /// The store type.
        /// </typeparam>
        /// <param name="modelType">
        /// The <see cref="ConverterBuilder{TModelType}"/> representing the model type.
        /// </param>
        /// <param name="decoder">
        /// The function used to decode the store type into a byte array.
        /// </param>
        /// <param name="encoder">
        /// The function used to encode a byte array into the store type.
        /// </param>
        /// <returns>
        /// An <see cref="ConverterBuilder{TModelType,TStoreType}"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="modelType"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="decoder"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="encoder"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="modelType"/> is not a supported type.
        /// </exception>
        public static ConverterBuilder<TModelType, TStoreType> To<TModelType, TStoreType>(
            ConverterBuilder<TModelType> modelType,
            Func<TStoreType, byte[]> decoder,
            Func<Stream, TStoreType> encoder)
        {
            if (modelType.IsEmpty)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            return new ConverterBuilder<TModelType, TStoreType>(modelType, decoder, encoder);
        }

        /// <summary>
        /// Specifies that the property should be stored in the database in binary.
        /// </summary>
        /// <typeparam name="TModelType">
        /// The model type.
        /// </typeparam>
        /// <param name="modelType">
        /// The <see cref="ConverterBuilder{TModelType}"/> representing the model type.
        /// </param>
        /// <returns>
        /// An <see cref="ConverterBuilder{TModelType,TStoreType}"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="modelType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="modelType"/> is not a supported type.
        /// </exception>
        public static ConverterBuilder<TModelType, byte[]> ToBinary<TModelType>(this ConverterBuilder<TModelType> modelType)
        {
            if (modelType.IsEmpty)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            return new ConverterBuilder<TModelType, byte[]>(modelType, b => b, StandardConverters.StreamToBytes);
        }

        /// <summary>
        /// Specifies that the property should be stored in the database in a Base64-encoded string.
        /// </summary>
        /// <typeparam name="TModelType">
        /// The model type.
        /// </typeparam>
        /// <param name="modelType">
        /// The <see cref="ConverterBuilder{TModelType}"/> representing the model type.
        /// </param>
        /// <returns>
        /// An <see cref="ConverterBuilder{TModelType,TStoreType}"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="modelType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="modelType"/> is not a supported type.
        /// </exception>
        public static ConverterBuilder<TModelType, string> ToBase64<TModelType>(this ConverterBuilder<TModelType> modelType)
        {
            if (modelType.IsEmpty)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            return new ConverterBuilder<TModelType, string>(modelType, Convert.FromBase64String, StandardConverters.StreamToBase64String);
        }
    }
}