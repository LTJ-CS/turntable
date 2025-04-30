using System.Linq;
using System.Security.Cryptography;

namespace Sdk.Runtime.Crypto
{
    public class AesCrypto:ICrypto
    {
        private readonly AesCryptoServiceProvider _aesProvider = new ();

        public byte[] Iv { get; set; }

        public AesCrypto(string iv)
        {
            Iv = System.Text.Encoding.UTF8.GetBytes(iv);
        }
        
        public byte[] Encrypt(string key, byte[] data)
        {
            
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            _aesProvider.Key = keyBytes.Take(32).ToArray();
            _aesProvider.IV = Iv;
            _aesProvider.Mode = CipherMode.CBC;
            _aesProvider.Padding = PaddingMode.PKCS7;
            var encryptor = _aesProvider.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public byte[] Decrypt(string key, byte[] data)
        {
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            _aesProvider.Key = keyBytes;
            _aesProvider.IV = Iv;
            _aesProvider.Mode = CipherMode.CBC;
            _aesProvider.Padding = PaddingMode.PKCS7;
            var decryptor = _aesProvider.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }
    }
}