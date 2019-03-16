namespace Microsoft.EntityFrameworkCore.DataEncryption.Providers
{
    /// <summary>
    /// Specifies the available AES Key sizes used for generating encryption keys and initialization vectors.
    /// </summary>
    /// <remarks>
    /// The key sizes are defined in bits.
    /// </remarks>
    public enum AesKeySize : uint
    {
        /// <summary>
        /// AES 128 bits key size.
        /// </summary>
        AES128Bits = 128,

        /// <summary>
        /// AES 192 bits key size.
        /// </summary>
        AES192Bits = 192,

        /// <summary>
        /// AES 256 bits key size.
        /// </summary>
        AES256Bits = 256
    }
}
