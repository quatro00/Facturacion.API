using Facturacion.API.Services.Interface;
using System.Text;
using System.Security.Cryptography;

namespace Facturacion.API.Services.Implementation
{
    public class AesCryptoService : ICryptoService
    {
        private readonly byte[] _key;

        public AesCryptoService(IConfiguration config)
        {
            _key = Convert.FromBase64String(config["Crypto:Key"]);
        }

        public byte[] Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return aes.IV.Concat(cipherBytes).ToArray();
        }

        public string Decrypt(byte[] cipherBytes)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var iv = cipherBytes.Take(16).ToArray();
            var data = cipherBytes.Skip(16).ToArray();
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(data, 0, data.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        public byte[] DecryptToByte(byte[] cipherBytes)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // IV = primeros 16 bytes
            var iv = cipherBytes.Take(16).ToArray();
            var data = cipherBytes.Skip(16).ToArray();

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(data, 0, data.Length);

            return plainBytes; // 👈 bytes reales
        }
    }
}
