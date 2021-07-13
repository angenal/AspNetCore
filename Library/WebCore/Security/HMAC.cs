using System;
using System.Text;
using System.Security.Cryptography;

namespace WebCore.Security
{
    /// <summary>
    /// 随机哈希算法 > Hmac算法也是一种哈希算法，它可以利用MD5或SHA1等哈希算法。
    ///   不同的是，Hmac还需要一个密钥；只要密钥发生了变化，那么同样的输入也会得到不同的签名。
    ///   因此，可以把Hmac理解为用随机数“增强”的哈希算法。
    /// </summary>
    public class HMAC
    {
        /// <summary>
        /// MD5 随机哈希算法
        /// </summary>
        /// <param name="data">输入</param>
        /// <param name="key">一个密钥:不区分大小写</param>
        /// <param name="encoding">文本编码:默认UTF8</param>
        /// <returns></returns>
        public static string MD5(string data, string key, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            string hashString;
            using (var hmac = new HMACMD5(encoding.GetBytes(key.ToUpper())))
            {
                var hash = hmac.ComputeHash(encoding.GetBytes(data));
                hashString = Convert.ToBase64String(hash);
            }
            return hashString;
        }
        /// <summary>
        /// SHA1 随机哈希算法
        /// </summary>
        /// <param name="data">输入</param>
        /// <param name="key">一个密钥:不区分大小写</param>
        /// <param name="encoding">文本编码:默认UTF8</param>
        /// <returns></returns>
        public static string SHA1(string data, string key, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            string hashString;
            using (var hmac = new HMACSHA1(encoding.GetBytes(key.ToUpper())))
            {
                var hash = hmac.ComputeHash(encoding.GetBytes(data));
                hashString = Convert.ToBase64String(hash);
            }
            return hashString;
        }
        /// <summary>
        /// SHA256 随机哈希算法
        /// </summary>
        /// <param name="data">输入</param>
        /// <param name="key">一个密钥:不区分大小写</param>
        /// <param name="encoding">文本编码:默认UTF8</param>
        /// <returns></returns>
        public static string SHA256(string data, string key, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            string hashString;
            using (var hmac = new HMACSHA256(encoding.GetBytes(key.ToUpper())))
            {
                var hash = hmac.ComputeHash(encoding.GetBytes(data));
                hashString = Convert.ToBase64String(hash);
            }
            return hashString;
        }
        /// <summary>
        /// SHA512 随机哈希算法
        /// </summary>
        /// <param name="data">输入</param>
        /// <param name="key">一个密钥:不区分大小写</param>
        /// <param name="encoding">文本编码:默认UTF8</param>
        /// <returns></returns>
        public static string SHA512(string data, string key, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            string hashString;
            using (var hmac = new HMACSHA512(encoding.GetBytes(key.ToUpper())))
            {
                var hash = hmac.ComputeHash(encoding.GetBytes(data));
                hashString = Convert.ToBase64String(hash);
            }
            return hashString;
        }
    }
}
