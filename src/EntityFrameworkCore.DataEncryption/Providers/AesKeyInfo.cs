using System;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Providers
{
    /// <summary>
    /// Defines an AES key info structure containing a Key and Initialization Vector used for the AES encryption algorithm.
    /// </summary>
    public readonly struct AesKeyInfo : IEquatable<AesKeyInfo>
    {
        /// <summary>
        /// Gets the AES key.
        /// </summary>
        public byte[] Key { get; }

        /// <summary>
        /// Gets the AES initialization vector.
        /// </summary>
        public byte[] IV { get; }

        /// <summary>
        /// Creates a new <see cref="AesKeyInfo"/>.
        /// </summary>
        /// <param name="key">AES key.</param>
        /// <param name="iv">AES initialization vector.</param>
        internal AesKeyInfo(byte[] key, byte[] iv)
        {
            Key = key;
            IV = iv;
        }

        /// <summary>
        /// Determines whether the current <see cref="AesKeyInfo"/> is equal to another <see cref="AesKeyInfo"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AesKeyInfo other) => (Key, IV) == (other.Key, other.IV);

        /// <summary>
        /// Determines whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) => (obj is AesKeyInfo keyInfo) && Equals(keyInfo);

        /// <summary>
        /// Calculates the hash code for the current <see cref="AesKeyInfo"/> instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => (Key, IV).GetHashCode();

        /// <summary>
        /// Determines whether the current <see cref="AesKeyInfo"/> is equal to another <see cref="AesKeyInfo"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(AesKeyInfo left, AesKeyInfo right) => Equals(left, right);

        /// <summary>
        /// Determines whether the current <see cref="AesKeyInfo"/> is not equal to another <see cref="AesKeyInfo"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(AesKeyInfo left, AesKeyInfo right) => !Equals(left, right);
    }
}
