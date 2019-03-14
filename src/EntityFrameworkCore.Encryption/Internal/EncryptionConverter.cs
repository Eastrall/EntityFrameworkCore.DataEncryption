using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Encryption.Internal
{
    /// <summary>
    /// Defines the internal encryption converter for string values.
    /// </summary>
    internal sealed class EncryptionConverter : ValueConverter<string, string>
    {
        /// <summary>
        /// Creates a new <see cref="EncryptionConverter"/> instance.
        /// </summary>
        /// <param name="encryptionProvider">Encryption provider</param>
        /// <param name="mappingHints">Entity Framework mapping hints</param>
        public EncryptionConverter(IEncryptionProvider encryptionProvider, ConverterMappingHints mappingHints = null) 
            : base(x => encryptionProvider.Encrypt(x), x => encryptionProvider.Decrypt(x), mappingHints)
        {
        }
    }
}
