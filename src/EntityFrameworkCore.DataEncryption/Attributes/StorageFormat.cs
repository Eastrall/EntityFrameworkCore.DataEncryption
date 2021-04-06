namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Represents the storage format for an encrypted value.
    /// </summary>
    public enum StorageFormat
    {
        /// <summary>
        /// The format is determined by the model data type.
        /// </summary>
        Default,
        /// <summary>
        /// The value is stored in binary.
        /// </summary>
        Binary,
        /// <summary>
        /// The value is stored in a Base64-encoded string.
        /// </summary>
        /// <remarks>
        /// <b>NB:</b> If the source property is a <see cref="string"/>,
        /// and no encryption provider is configured,
        /// the string will not be modified.
        /// </remarks>
        Base64,
    }
}