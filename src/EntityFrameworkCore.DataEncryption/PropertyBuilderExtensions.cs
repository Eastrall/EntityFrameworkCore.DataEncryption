using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.DataEncryption
{
    /// <summary>
    /// Provides extensions for the <see cref="PropertyBuilder"/>.
    /// </summary>
    public static class PropertyBuilderExtensions
    {
        /// <summary>
        /// Configures the property as capable of storing encrypted data.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="encryptionProvider">The encryption provider to use .</param>
        /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
        public static PropertyBuilder<string> IsEncrypted(this PropertyBuilder<string> propertyBuilder, IEncryptionProvider encryptionProvider)
        {
            if (encryptionProvider == null) return propertyBuilder;

            var encryptionConverter = new EncryptionConverter(encryptionProvider);

            return propertyBuilder.HasConversion(encryptionConverter);
        }
    }
}