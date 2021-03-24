using System;
using System.IO;
using System.Security;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal
{
	/// <summary>
	/// Utilities for building value converters.
	/// </summary>
	public static class ConverterBuilder
	{
		#region Interfaces

		/// <summary>
		/// Interface for a builder class which has the model type specified.
		/// </summary>
		/// <typeparam name="TModelType">
		/// The model type.
		/// </typeparam>
		// ReSharper disable once UnusedTypeParameter
		public interface INeedStoreType<TModelType> { }

		/// <summary>
		/// Interface for a builder class with both the model and store types specified.
		/// </summary>
		/// <typeparam name="TModelType">
		/// The model type.
		/// </typeparam>
		/// <typeparam name="TStoreType">
		/// The store type.
		/// </typeparam>
		public interface IBuilder<TModelType, TStoreType>
		{
			/// <summary>
			/// Builds the value converter.
			/// </summary>
			/// <param name="mappingHints">
			/// The mapping hints to use, if any.
			/// </param>
			/// <returns>
			/// The <see cref="ValueConverter{TModel,TProvider}"/>.
			/// </returns>
			ValueConverter<TModelType, TStoreType> Build(ConverterMappingHints mappingHints = null);
		}

		#endregion

		#region Encrypting

		private sealed class NeedStoreType<TModelType> : INeedStoreType<TModelType>
		{
			public NeedStoreType(IEncryptionProvider encryptionProvider, Func<TModelType, byte[]> decoder, Func<Stream, TModelType> encoder)
			{
				EncryptionProvider = encryptionProvider ?? throw new ArgumentNullException(nameof(encryptionProvider));
				Decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
				Encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
			}

			private IEncryptionProvider EncryptionProvider { get; }
			private Func<TModelType, byte[]> Decoder { get; }
			private Func<Stream, TModelType> Encoder { get; }

			public void Deconstruct(out IEncryptionProvider encryptionProvider, out Func<TModelType, byte[]> decoder, out Func<Stream, TModelType> encoder)
			{
				encryptionProvider = EncryptionProvider;
				decoder = Decoder;
				encoder = Encoder;
			}
		}

		private sealed class EncryptionBuilder<TModelType, TStoreType> : IBuilder<TModelType, TStoreType>
		{
			public EncryptionBuilder(NeedStoreType<TModelType> modelType, Func<TStoreType, byte[]> decoder, Func<Stream, TStoreType> encoder)
			{
				ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
				Decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
				Encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
			}

			private NeedStoreType<TModelType> ModelType { get; }
			private Func<TStoreType, byte[]> Decoder { get; }
			private Func<Stream, TStoreType> Encoder { get; }

			/// <inheritdoc />
			public ValueConverter<TModelType, TStoreType> Build(ConverterMappingHints mappingHints = null)
			{
				var (encryptionProvider, modelDecoder, modelEncoder) = ModelType;
				var storeDecoder = Decoder;
				var storeEncoder = Encoder;

				return new EncryptionConverter<TModelType, TStoreType>(
					encryptionProvider,
					m => encryptionProvider.Encrypt(m, modelDecoder, storeEncoder),
					s => encryptionProvider.Decrypt(s, storeDecoder, modelEncoder),
					mappingHints);
			}
		}

		#endregion

		#region Non-encrypting

		private sealed class ByteConverter<T> : INeedStoreType<T>
		{
			public ByteConverter(Func<T, byte[]> decoder, Func<byte[], T> encoder)
			{
				Decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
				Encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
			}

			private Func<T, byte[]> Decoder { get; }
			private Func<byte[], T> Encoder { get; }

			public void Deconstruct(out Func<T, byte[]> decoder, out Func<byte[], T> encoder)
			{
				decoder = Decoder;
				encoder = Encoder;
			}
		}

		private static class ByteConverter
		{
			public static ByteConverter<byte[]> Identity { get; } = new(b => b, b => b);
			public static ByteConverter<string> Base64String { get; } = new(Convert.FromBase64String, Convert.ToBase64String);
			public static ByteConverter<string> Utf8String { get; } = new(Encoding.UTF8.GetBytes, Encoding.UTF8.GetString);
			public static ByteConverter<SecureString> Utf8SecureString { get; } = new(Encoding.UTF8.GetBytes, Encoding.UTF8.GetSecureString);

			public static Stream WrapBytes(byte[] bytes) => new MemoryStream(bytes);
		}

		private sealed class NonEncryptionBuilder<TModelType, TStoreType> : IBuilder<TModelType, TStoreType>
		{
			public NonEncryptionBuilder(ByteConverter<TModelType> modelConverter, ByteConverter<TStoreType> storeConverter)
			{
				ModelConverter = modelConverter ?? throw new ArgumentNullException(nameof(modelConverter));
				StoreConverter = storeConverter ?? throw new ArgumentNullException(nameof(storeConverter));
			}

			private ByteConverter<TModelType> ModelConverter { get; }
			private ByteConverter<TStoreType> StoreConverter { get; }

			/// <inheritdoc />
			public ValueConverter<TModelType, TStoreType> Build(ConverterMappingHints mappingHints = null)
			{
				var (modelDecoder, modelEncoder) = ModelConverter;
				var (storeDecoder, storeEncoder) = StoreConverter;
				return new ValueConverter<TModelType, TStoreType>(m => storeEncoder(modelDecoder(m)), s => modelEncoder(storeDecoder(s)), mappingHints);
			}
		}

		#endregion

		#region Standard Converters

		internal static byte[] StreamToBytes(Stream stream)
		{
			if (stream is MemoryStream ms) return ms.ToArray();

			using var output = new MemoryStream();
			stream.CopyTo(output);
			return output.ToArray();
		}

		internal static string StreamToBase64String(Stream stream) => Convert.ToBase64String(StreamToBytes(stream));

		internal static string StreamToString(Stream stream)
		{
			using var reader = new StreamReader(stream, Encoding.UTF8);
			return reader.ReadToEnd().Trim('\0');
		}

		internal static SecureString StreamToSecureString(Stream stream)
		{
			using var reader = new StreamReader(stream, Encoding.UTF8);

			var result = new SecureString();
			var buffer = new char[100];
			while (!reader.EndOfStream)
			{
				var charsRead = reader.Read(buffer, 0, buffer.Length);
				if (charsRead != 0)
				{
					for (int index = 0; index < charsRead; index++)
					{
						char c = buffer[index];
						if (c != '\0') result.AppendChar(c);
					}
				}
			}

			return result;
		}

		#endregion

		#region Builders

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
		/// An <see cref="INeedStoreType{TModelType}"/> instance.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="decoder"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="encoder"/> is <see langword="null"/>.</para>
		/// </exception>
		public static INeedStoreType<TModelType> From<TModelType>(
			this IEncryptionProvider encryptionProvider,
			Func<TModelType, byte[]> decoder,
			Func<Stream, TModelType> encoder)
		{
			if (decoder is null) throw new ArgumentNullException(nameof(decoder));
			if (encoder is null) throw new ArgumentNullException(nameof(encoder));
			if (encryptionProvider is not null) return new NeedStoreType<TModelType>(encryptionProvider, decoder, encoder);
			return new ByteConverter<TModelType>(decoder, b => encoder(ByteConverter.WrapBytes(b)));
		}

		/// <summary>
		/// Builds a converter for a binary property.
		/// </summary>
		/// <param name="encryptionProvider">
		/// The <see cref="IEncryptionProvider"/>, if any.
		/// </param>
		/// <returns>
		/// An <see cref="INeedStoreType{TModelType}"/> instance.
		/// </returns>
		public static INeedStoreType<byte[]> FromBinary(this IEncryptionProvider encryptionProvider)
		{
			if (encryptionProvider is null) return ByteConverter.Identity;
			return new NeedStoreType<byte[]>(encryptionProvider, b => b, StreamToBytes);
		}

		/// <summary>
		/// Builds a converter for a string property.
		/// </summary>
		/// <param name="encryptionProvider">
		/// The <see cref="IEncryptionProvider"/>, if any.
		/// </param>
		/// <returns>
		/// An <see cref="INeedStoreType{TModelType}"/> instance.
		/// </returns>
		public static INeedStoreType<string> FromString(this IEncryptionProvider encryptionProvider)
		{
			if (encryptionProvider is null) return ByteConverter.Utf8String;
			return new NeedStoreType<string>(encryptionProvider, Encoding.UTF8.GetBytes, StreamToString);
		}

		/// <summary>
		/// Builds a converter for a <see cref="SecureString"/> property.
		/// </summary>
		/// <param name="encryptionProvider">
		/// The <see cref="IEncryptionProvider"/>, if any.
		/// </param>
		/// <returns>
		/// An <see cref="INeedStoreType{TModelType}"/> instance.
		/// </returns>
		public static INeedStoreType<SecureString> FromSecureString(this IEncryptionProvider encryptionProvider)
		{
			if (encryptionProvider is null) return ByteConverter.Utf8SecureString;
			return new NeedStoreType<SecureString>(encryptionProvider, Encoding.UTF8.GetBytes, StreamToSecureString);
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
		/// The <see cref="INeedStoreType{TModelType}"/> representing the model type.
		/// </param>
		/// <param name="decoder">
		/// The function used to decode the store type into a byte array.
		/// </param>
		/// <param name="encoder">
		/// The function used to encode a byte array into the store type.
		/// </param>
		/// <returns>
		/// An <see cref="IBuilder{TModelType,TStoreType}"/> instance.
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
		public static IBuilder<TModelType, TStoreType> To<TModelType, TStoreType>(
			INeedStoreType<TModelType> modelType,
			Func<TStoreType, byte[]> decoder,
			Func<Stream, TStoreType> encoder)
		{
			if (modelType is null) throw new ArgumentNullException(nameof(modelType));
			if (decoder is null) throw new ArgumentNullException(nameof(decoder));
			if (encoder is null) throw new ArgumentNullException(nameof(encoder));

			return modelType switch
			{
				ByteConverter<TModelType> converter => new NonEncryptionBuilder<TModelType, TStoreType>(converter, new ByteConverter<TStoreType>(decoder, b => encoder(ByteConverter.WrapBytes(b)))),
				NeedStoreType<TModelType> converter => new EncryptionBuilder<TModelType, TStoreType>(converter, decoder, encoder),
				_ => throw new ArgumentException($"Unsupported model type: {modelType}", nameof(modelType)),
			};
		}

		/// <summary>
		/// Specifies that the property should be stored in the database in binary.
		/// </summary>
		/// <typeparam name="TModelType">
		/// The model type.
		/// </typeparam>
		/// <param name="modelType">
		/// The <see cref="INeedStoreType{TModelType}"/> representing the model type.
		/// </param>
		/// <returns>
		/// An <see cref="IBuilder{TModelType,TStoreType}"/> instance.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="modelType"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="modelType"/> is not a supported type.
		/// </exception>
		public static IBuilder<TModelType, byte[]> ToBinary<TModelType>(this INeedStoreType<TModelType> modelType) => modelType switch
		{
			// ReSharper disable once HeuristicUnreachableCode
			null => throw new ArgumentNullException(nameof(modelType)),
			ByteConverter<TModelType> converter => new NonEncryptionBuilder<TModelType, byte[]>(converter, ByteConverter.Identity),
			NeedStoreType<TModelType> converter => new EncryptionBuilder<TModelType, byte[]>(converter, b => b, StreamToBytes),
			_ => throw new ArgumentException($"Unsupported model type: {modelType}", nameof(modelType)),
		};

		/// <summary>
		/// Specifies that the property should be stored in the database in a Base64-encoded string.
		/// </summary>
		/// <typeparam name="TModelType">
		/// The model type.
		/// </typeparam>
		/// <param name="modelType">
		/// The <see cref="INeedStoreType{TModelType}"/> representing the model type.
		/// </param>
		/// <returns>
		/// An <see cref="IBuilder{TModelType,TStoreType}"/> instance.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="modelType"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="modelType"/> is not a supported type.
		/// </exception>
		public static IBuilder<TModelType, string> ToBase64<TModelType>(this INeedStoreType<TModelType> modelType) => modelType switch
		{
			// ReSharper disable once HeuristicUnreachableCode
			null => throw new ArgumentNullException(nameof(modelType)),
			ByteConverter<TModelType> converter => new NonEncryptionBuilder<TModelType, string>(converter, ByteConverter.Base64String),
			NeedStoreType<TModelType> converter => new EncryptionBuilder<TModelType, string>(converter, Convert.FromBase64String, StreamToBase64String),
			_ => throw new ArgumentException($"Unsupported model type: {modelType}", nameof(modelType)),
		};

		#endregion
	}
}