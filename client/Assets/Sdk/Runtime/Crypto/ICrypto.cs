namespace Sdk.Runtime.Crypto
{
    /// <summary>
    /// 加密算法接口
    /// </summary>
    public interface ICrypto
    {
        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="key">加密 key</param>
        /// <param name="data"> 需要加密的数据</param>
        /// <returns></returns>
        public byte[] Encrypt(string key, byte[] data);
        
        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="key">解密 key</param>
        /// <param name="data">需要解密的数据</param>
        /// <returns></returns>
        public byte[] Decrypt(string key, byte[] data);
    }
}