namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Specifies that the data field value should be encrypted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class EncryptedAttribute : Attribute
    {
    }
}
