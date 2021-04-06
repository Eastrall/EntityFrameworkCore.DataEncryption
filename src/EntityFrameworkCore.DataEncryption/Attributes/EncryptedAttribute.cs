namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Specifies that the data field value should be encrypted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class EncryptedAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedAttribute"/> class.
        /// </summary>
        /// <param name="format">
        /// The storage format.
        /// </param>
        public EncryptedAttribute(StorageFormat format)
        {
            Format = format;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedAttribute"/> class.
        /// </summary>
        public EncryptedAttribute() : this(StorageFormat.Default)
        {
        }

        /// <summary>
        /// Returns the storage format for the database value.
        /// </summary>
        public StorageFormat Format { get; }
    }
}