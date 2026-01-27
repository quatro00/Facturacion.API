namespace Facturacion.API.Services.Interface
{
    public interface ICryptoService
    {
        byte[] Encrypt(string plainText);
        string Decrypt(byte[] cipherBytes);
        byte[] DecryptToByte(byte[] cipherBytes);
    }
}
