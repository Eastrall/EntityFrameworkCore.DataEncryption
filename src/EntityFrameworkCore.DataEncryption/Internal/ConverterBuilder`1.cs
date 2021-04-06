using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal
{
    /// <summary>
    /// A converter builder class which has the model type specified.
    /// </summary>
    /// <typeparam name="TModelType">
    /// The model type.
    /// </typeparam>
    public readonly struct ConverterBuilder<TModelType>
    {
        internal ConverterBuilder(IEncryptionProvider encryptionProvider, Func<TModelType, byte[]> decoder, Func<Stream, TModelType> encoder)
        {
            Debug.Assert(decoder is not null);
            Debug.Assert(encoder is not null);

            EncryptionProvider = encryptionProvider;
            Decoder = decoder;
            Encoder = encoder;
        }

        private IEncryptionProvider EncryptionProvider { get; }
        private Func<TModelType, byte[]> Decoder { get; }
        private Func<Stream, TModelType> Encoder { get; }
        internal bool IsEmpty => Decoder is null || Encoder is null;

        internal void Deconstruct(out IEncryptionProvider encryptionProvider, out Func<TModelType, byte[]> decoder, out Func<Stream, TModelType> encoder)
        {
            encryptionProvider = EncryptionProvider;
            decoder = Decoder;
            encoder = Encoder;
        }
    }
}