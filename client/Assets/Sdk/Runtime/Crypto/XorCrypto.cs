using System.Text;

namespace Sdk.Runtime.Crypto
{
    public class XorCrypto:ICrypto
    {
        public byte[] Encrypt(string key, byte[] data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            // 用 key 的每个字节对 data 的每个字节进行异或运算
            for (var i = 0; i < data.Length; i++)
            {
                data[i] ^= keyBytes[i % keyBytes.Length];
            }

            return data;
        }

        public byte[] Decrypt(string key, byte[] data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            // 用 key 的每个字节对 data 的每个字节进行异或运算
            for (var i = 0; i < data.Length; i++)
            {
                data[i] ^= keyBytes[i % keyBytes.Length];
            }
            return data;
        }
    }
}