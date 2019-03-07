namespace Microsoft.EntityFrameworkCore.Encryption
{
    public interface IEncryptionProvider
    {
        string Encrypt(string dataToEncrypt);

        string Decrypt(string dataToDecrypt);
    }
}
