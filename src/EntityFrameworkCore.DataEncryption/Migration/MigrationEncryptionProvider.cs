using System;
using System.IO;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Migration
{
    /// <summary>
    /// An encryption provided used for migrating from one encryption scheme to another.
    /// </summary>
    public class MigrationEncryptionProvider : IEncryptionProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationEncryptionProvider" /> class.
        /// </summary>
        /// <param name="sourceEncryptionProvider">The source encryption provider.</param>
        /// <param name="destinationEncryptionProvider">The destination encryption provider.</param>
        public MigrationEncryptionProvider(
            IEncryptionProvider sourceEncryptionProvider,
            IEncryptionProvider destinationEncryptionProvider)
        {
            SourceEncryptionProvider = sourceEncryptionProvider;
            DestinationEncryptionProvider = destinationEncryptionProvider;
        }

        /// <summary>
        /// Returns the original encryption provider, if any.
        /// </summary>
        /// <value>
        /// The original <see cref="IEncryptionProvider"/>, if any.
        /// </value>
        public IEncryptionProvider SourceEncryptionProvider { get; }

        /// <summary>
        /// Returns the new encryption provider, if any.
        /// </summary>
        /// <value>
        /// The new <see cref="IEncryptionProvider"/>, if any.
        /// </value>
        public IEncryptionProvider DestinationEncryptionProvider { get; }

        /// <summary>
        /// Returns a flag indicating whether this provider is empty.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this provider is empty;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool IsEmpty => SourceEncryptionProvider is null && DestinationEncryptionProvider is null;

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

            if (SourceEncryptionProvider is not null)
            {
                return SourceEncryptionProvider.Decrypt(dataToDecrypt, decoder, converter);
            }

            byte[] data = decoder(dataToDecrypt);
            if (data is null || data.Length == 0)
            {
                return default;
            }

            using var ms = new MemoryStream(data);
            return converter(ms);
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

            if (DestinationEncryptionProvider is not null)
            {
                return DestinationEncryptionProvider.Encrypt(dataToEncrypt, converter, encoder);
            }

            byte[] data = converter(dataToEncrypt);
            if (data is null || data.Length == 0)
            {
                return default;
            }

            using var ms = new MemoryStream(data);
            return encoder(ms);
        }
    }
}