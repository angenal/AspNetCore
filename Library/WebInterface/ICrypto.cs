using System.Security.Cryptography;

namespace WebInterface
{
    /// <summary>
    /// Cryptography methods.
    /// </summary>
    public interface ICrypto
    {
        /// <summary>
        /// Get encrypted data with AES Rijndael
        /// </summary>
        /// <param name="password">Original data</param>
        /// <param name="key">Key</param>
        /// <param name="iv">Initialization vector (start vector)</param>
        /// <param name="mode"></param>
        /// <param name="padding"></param>
        /// <param name="keySize"></param>
        /// <param name="blockSize"></param>
        /// <returns>Encrypted data</returns>
        byte[] AESEncrypt(byte[] password, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.ISO10126, int keySize = 256, int blockSize = 128);
        /// <summary>
        /// Get decrypted data with AES Rijndael
        /// </summary>
        /// <param name="hashedPassword">Encrypted data</param>
        /// <param name="key">Key</param>
        /// <param name="iv">Initialization vector (start vector)</param>
        /// <param name="mode"></param>
        /// <param name="padding"></param>
        /// <param name="keySize"></param>
        /// <param name="blockSize"></param>
        /// <returns>Original data</returns>
        byte[] AESDecrypt(byte[] hashedPassword, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.ISO10126, int keySize = 256, int blockSize = 128);
        /// <summary>
        /// AES + CEB Encryption.
        /// </summary>
        /// <param name="password">The password to generate a hash value for.</param>
        /// <param name="key"></param>
        /// <returns></returns>
        string AESEncrypt(string password, string key);
        /// <summary>
        /// AES + CBC Encryption.
        /// </summary>
        /// <param name="password">The password to generate a hash value for.</param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        string AESEncrypt(string password, string key, string iv);
        /// <summary>
        /// AES + CEB Decryption.
        /// </summary>
        /// <param name="hashedPassword">The previously-computed hash value as a base-64-encoded string.</param>
        /// <param name="key"></param>
        /// <returns></returns>
        string AESDecrypt(string hashedPassword, string key);
        /// <summary>
        /// AES + CBC Decryption.
        /// </summary>
        /// <param name="hashedPassword">The previously-computed hash value as a base-64-encoded string.</param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        string AESDecrypt(string hashedPassword, string key, string iv);

        /// <summary>
        /// New RSA Instance.
        /// </summary>
        /// <param name="keySize"></param>
        /// <param name="persistKeyInCsp"></param>
        /// <param name="keyContainerName"></param>
        /// <returns></returns>
        RSACryptoServiceProvider NewRSA(int keySize = 1024, bool persistKeyInCsp = false, string keyContainerName = null);
        /// <summary>
        /// Get encrypted data with RSA
        /// </summary>
        /// <param name="password">Original data</param>
        /// <param name="optimalAsymmetricEncryptionPadding"></param>
        /// <returns>Encrypted data</returns>
        byte[] RSAEncrypt(byte[] password, bool optimalAsymmetricEncryptionPadding = true);
        /// <summary>
        /// Get decrypted data with RSA
        /// </summary>
        /// <param name="hashedPassword">Encrypted data</param>
        /// <param name="optimalAsymmetricEncryptionPadding"></param>
        /// <returns>Original data</returns>
        byte[] RSADecrypt(byte[] hashedPassword, bool optimalAsymmetricEncryptionPadding = true);
        /// <summary>
        /// Output RSA PrivateKey (for RSADecrypt) and PublicKey (for RSAEncrypt)
        /// </summary>
        string ToRSAXmlString(bool privateKey);
        /// <summary>
        /// Input RSA PrivateKey (for RSADecrypt) and PublicKey (for RSAEncrypt)
        /// </summary>
        void FromRSAXmlString(string key = null);
        /// <summary>
        /// Get encrypted data with ( RSA + AES )
        /// </summary>
        /// <param name="password">Original data</param>
        /// <param name="aesKeySize"></param>
        /// <param name="aesBlockSize"></param>
        /// <returns>Encrypted data</returns>
        byte[] RSA2Encrypt(byte[] password, int aesKeySize = 256, int aesBlockSize = 128);
        /// <summary>
        /// Get decrypted data with ( RSA + AES )
        /// </summary>
        /// <param name="hashedPassword">Encrypted data</param>
        /// <returns>Original data</returns>
        byte[] RSA2Decrypt(byte[] hashedPassword);

        /// <summary>
        /// Get random or rastgele bytes.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        byte[] RandomBytes(int length);

        uint XXH32(string text);
        ulong XXH64(string text);
        ushort Crc16(string text);
        uint Crc32(string text);
        string Crc32x8(string text);
        string Crc32X8(string text);

        /// <summary>
        /// MD5 Encryption.
        /// </summary>
        /// <param name="password">The password to generate a hash value for.</param>
        /// <returns></returns>
        string Md5(string password);

        /// <summary>
        /// Sha1 Encryption.
        /// </summary>
        /// <param name="password">The password to generate a hash value for.</param>
        /// <returns></returns>
        string Sha1(string password);

        /// <summary>
        /// 随机哈希算法 > Hmac算法也是一种哈希算法，它可以利用MD5或SHA1等哈希算法。
        ///   不同的是，Hmac还需要一个密钥；只要密钥发生了变化，那么同样的输入也会得到不同的签名。
        ///   因此，可以把Hmac理解为用随机数“增强”的哈希算法。
        ///
        /// HS256 = HMACSHA256 (HMAC+SHA256) Encryption.
        /// </summary>
        /// <param name="password">The password to generate a hash value for.</param>
        /// <param name="key"></param>
        /// <returns></returns>
        string HS256(string password, string key);
        /// <summary>
        /// HS512 = HMACSHA512
        /// </summary>
        /// <param name="password">The password to generate a hash value for.</param>
        /// <param name="key"></param>
        /// <returns></returns>
        string HS512(string password, string key);
        /// <summary>
        /// HS1 = HMACSHA1
        /// </summary>
        /// <param name="password">The password to generate a hash value for.</param>
        /// <param name="key"></param>
        /// <returns></returns>
        string HS1(string password, string key);
        /// <summary>
        /// HMD5 = HMACMD5
        /// </summary>
        /// <param name="password">The password to generate a hash value for.</param>
        /// <param name="key"></param>
        /// <returns></returns>
        string HMD5(string password, string key);

        /// <summary>
        /// Returns a hashed representation of the specified password. See <see href="https://github.com/henkmollema/CryptoHelper">CryptoHelper</see>
        /// </summary>
        /// <param name="password">The password to generate a hash value for.</param>
        /// <returns>The hash value for password as a base-64-encoded string.</returns>
        string HashPassword(string password);

        /// <summary>
        /// Determines whether the specified RFC 2898 hash and password are a cryptographic match. See <see href="https://github.com/henkmollema/CryptoHelper">CryptoHelper</see>
        /// </summary>
        /// <param name="hashedPassword">The previously-computed RFC 2898 hash value as a base-64-encoded string.</param>
        /// <param name="password">The plaintext password to cryptographically compare with hashedPassword.</param>
        /// <returns>true if the hash value is a cryptographic match for the password; otherwise, false.</returns>
        bool VerifyHashedPassword(string hashedPassword, string password);
    }
}
