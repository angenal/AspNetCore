using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebCore.Security;

namespace WebCore
{
    /// <summary>
    /// 加密库 Cryptography 快捷操作
    /// </summary>
    public static class CryptoFunctions
    {
        //public static uint XXH(this byte[] bytes) => BitConverter.ToUInt32(new K4os.Hash.xxHash.XXH32().AsHashAlgorithm().ComputeHash(bytes), 0);
        /// <summary>
        /// XXH32
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static uint XXH32(this byte[] bytes) => K4os.Hash.xxHash.XXH32.DigestOf(bytes.AsSpan());
        /// <summary>
        /// XXH32
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static uint XXH32(this string text) { var bytes = Encoding.UTF8.GetBytes(text); return K4os.Hash.xxHash.XXH32.DigestOf(bytes, 0, bytes.Length); }
        /// <summary>
        /// XXH64
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ulong XXH64(this byte[] bytes) => K4os.Hash.xxHash.XXH64.DigestOf(bytes.AsSpan());
        /// <summary>
        /// XXH64
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ulong XXH64(this string text) { var bytes = Encoding.UTF8.GetBytes(text); return K4os.Hash.xxHash.XXH64.DigestOf(bytes, 0, bytes.Length); }
        /// <summary>
        /// Crc16
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ushort Crc16(this string text) => Crc16Algorithm.Crc16(Encoding.ASCII.GetBytes(text));
        /// <summary>
        /// Crc32
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static uint Crc32(this string text) => Crc32Algorithm.Crc32(Encoding.ASCII.GetBytes(text));
        /// <summary>
        /// Crc32.ToString("x8")
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Crc32x8(this string text) => Crc32(text).ToString("x8");
        /// <summary>
        /// Crc32.ToString("X8")
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Crc32X8(this string text) => Crc32(text).ToString("X8");



        #region chacha20-ietf-poly1305

        #endregion

        #region aes-256-gcm

        #endregion

        #region AES
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
        public static byte[] AESEncrypt(this byte[] password, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.ISO10126, int keySize = 256, int blockSize = 128) => Crypto.Instance.AESEncrypt(password, key, iv, mode, padding, keySize, blockSize);
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
        public static byte[] AESDecrypt(this byte[] hashedPassword, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.ISO10126, int keySize = 256, int blockSize = 128) => Crypto.Instance.AESDecrypt(hashedPassword, key, iv, mode, padding, keySize, blockSize);
        #endregion

        #region AES + CBC
        /// <summary>
        /// AESEncrypt
        /// </summary>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string AESEncrypt(this string password, string key, string iv) => Crypto.Instance.AESEncrypt(password, key, iv);
        /// <summary>
        /// AESDecrypt
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string AESDecrypt(this string hashedPassword, string key, string iv) => Crypto.Instance.AESDecrypt(hashedPassword, key, iv);
        #endregion

        #region AES + CEB
        /// <summary>
        /// AESEncrypt
        /// </summary>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AESEncrypt(this string password, string key) => Crypto.Instance.AESEncrypt(password, key);
        /// <summary>
        /// AESDecrypt
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AESDecrypt(this string hashedPassword, string key) => Crypto.Instance.AESDecrypt(hashedPassword, key);
        #endregion


        #region RSA

        /*
        string text = "{ original data }";
        byte[] original = ASCIIEncoding.UTF8.GetBytes(text);
        byte[] encrypted = original.RSAEncrypt();

        string privateKey = Crypto.Instance.RSAPrivateKey;
        byte[] decrypted = encrypted.RSADecrypt(privateKey.RSA());

        Console.WriteLine("original\t: {0}", text);
        Console.WriteLine("encrypted\t: {0}", Convert.ToBase64String(encrypted));
        Console.WriteLine("decrypted\t: {0}", ASCIIEncoding.UTF8.GetString(decrypted));
        */

        /// <summary>
        /// Creat RSA Instance with Private Key or Public Key
        /// </summary>
        /// <param name="xmlString"></param>
        /// <param name="keySize"></param>
        /// <param name="persistKeyInCsp"></param>
        /// <param name="keyContainerName"></param>
        /// <returns></returns>
        public static RSACryptoServiceProvider RSA(this string xmlString, int keySize = 1024, bool persistKeyInCsp = false, string keyContainerName = null)
        {
            var rsa = Crypto.RSA(keySize, persistKeyInCsp, keyContainerName);
            rsa.FromXmlString(xmlString);
            return rsa;
        }
        /// <summary>
        /// Creat RSA Instance with Private Key  https://github.com/dvsekhvalnov/jose-jwt
        /// </summary>
        /// <param name="fileName_p12">Private Key file*.p12</param>
        /// <param name="password"></param>
        /// <param name="keyStorageFlags">Private keys are stored in the local computer store rather than the current user store.</param>
        /// <returns></returns>
        public static RSACryptoServiceProvider RSA(this string fileName_p12, string password, X509KeyStorageFlags keyStorageFlags = X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet) => new X509Certificate2(fileName_p12, password, keyStorageFlags).GetRSAPrivateKey() as RSACryptoServiceProvider;
        /// <summary>
        /// Creat RSA Instance with Private Key  https://github.com/dvsekhvalnov/jose-jwt
        /// </summary>
        /// <param name="rawData_p12">Private Key raw data</param>
        /// <param name="password"></param>
        /// <param name="keyStorageFlags">Private keys are stored in the local computer store rather than the current user store.</param>
        /// <returns></returns>
        public static RSACryptoServiceProvider RSA(this byte[] rawData_p12, string password, X509KeyStorageFlags keyStorageFlags = X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet) => new X509Certificate2(rawData_p12, password, keyStorageFlags).PrivateKey as RSACryptoServiceProvider;
        /// <summary>
        /// Get RSA Private Key and Public Key
        /// </summary>
        /// <returns></returns>
        public static (string PrivateKey, string PublicKey) Keys(this RSACryptoServiceProvider rsa) => (rsa.ToXmlString(true), rsa.ToXmlString(false));
        //public static (string PrivateKey, string PublicKey) RSAKeys() => (Crypto.Instance.rsa.ToXmlString(true), Crypto.Instance.rsa.ToXmlString(false));
        /// <summary>
        /// Get encrypted data with RSA
        /// </summary>
        /// <param name="password">Original data</param>
        /// <param name="optimalAsymmetricEncryptionPadding"></param>
        /// <returns>Encrypted data</returns>
        public static byte[] RSAEncrypt(this byte[] password, bool optimalAsymmetricEncryptionPadding = true) => Crypto.Instance.rsa.Encrypt(password, optimalAsymmetricEncryptionPadding);
        /// <summary>
        /// Get encrypted data with RSA
        /// </summary>
        /// <param name="password">Original data</param>
        /// <param name="rsa">RSA Instance with Private Key or Public Key</param>
        /// <param name="optimalAsymmetricEncryptionPadding"></param>
        /// <returns>Encrypted data</returns>
        public static byte[] RSAEncrypt(this byte[] password, RSACryptoServiceProvider rsa, bool optimalAsymmetricEncryptionPadding = true) => rsa.Encrypt(password, optimalAsymmetricEncryptionPadding);
        /// <summary>
        /// Get decrypted data with RSA
        /// </summary>
        /// <param name="hashedPassword">Encrypted data</param>
        /// <param name="optimalAsymmetricEncryptionPadding"></param>
        /// <returns>Original data</returns>
        public static byte[] RSADecrypt(this byte[] hashedPassword, bool optimalAsymmetricEncryptionPadding = true) => Crypto.Instance.rsa.Decrypt(hashedPassword, optimalAsymmetricEncryptionPadding);
        /// <summary>
        /// Get decrypted data with RSA
        /// </summary>
        /// <param name="hashedPassword">Encrypted data</param>
        /// <param name="rsa">RSA Instance with Private Key or Public Key</param>
        /// <param name="optimalAsymmetricEncryptionPadding"></param>
        /// <returns></returns>
        public static byte[] RSADecrypt(this byte[] hashedPassword, RSACryptoServiceProvider rsa, bool optimalAsymmetricEncryptionPadding = true) => rsa.Decrypt(hashedPassword, optimalAsymmetricEncryptionPadding);
        /// <summary>
        /// Get Specify maximum data length to encrypt RSA
        /// </summary>
        public static int MaxRSAKeySize(this int keySize, bool asymmetricEncryptionPadding = true) => Crypto.MaxRSAKeySize(keySize, asymmetricEncryptionPadding);
        /// <summary>
        /// Check RSA Key Size
        /// </summary>
        public static bool IsRSAKeySize(this int keySize) => Crypto.IsRSAKeySize(keySize);
        #endregion

        #region RSA + AES
        /// <summary>
        /// Get encrypted data with ( RSA + AES )
        /// </summary>
        /// <param name="password">Original data</param>
        /// <param name="aesKeySize"></param>
        /// <param name="aesBlockSize"></param>
        /// <returns>Encrypted data</returns>
        public static byte[] RSA2Encrypt(this byte[] password, int aesKeySize = 256, int aesBlockSize = 128) => Crypto.Instance.RSA2Encrypt(password, aesKeySize, aesBlockSize);
        /// <summary>
        /// Get decrypted data with ( RSA + AES )
        /// </summary>
        /// <param name="hashedPassword">Encrypted data</param>
        /// <returns>Original data</returns>
        public static byte[] RSA2Decrypt(this byte[] hashedPassword) => Crypto.Instance.RSA2Decrypt(hashedPassword);
        #endregion


        /// <summary>
        /// Generates and returns a random or rastgele bytes.
        /// </summary>
        /// <param name="length">Length of the bytes to be returned.</param>
        /// <returns></returns>
        public static byte[] RandomBytes(this int length) => Crypto.Instance.RandomBytes(length);
        /// <summary>
        /// Generates and returns a random sequence of strings
        /// </summary>
        /// <param name="length">Length of the string to be returned.</param>
        /// <returns>Captcha string</returns>
        public static string RandomString(this int length) => Crypto.Instance.RandomString(length);

        /// <summary>
        /// Md5
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Md5(this string password) => Crypto.Instance.Md5(password);
        /// <summary>
        /// Sha1
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Sha1(this string password) => Crypto.Instance.Sha1(password);
        /// <summary>
        /// HS256
        /// </summary>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string HS256(this string password, string key) => Crypto.Instance.HS256(password, key);
        /// <summary>
        /// HashPassword  See <see href="https://github.com/henkmollema/CryptoHelper">CryptoHelper</see>
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HashPassword(this string password) => CryptoHelper.Crypto.HashPassword(password);
        /// <summary>
        /// VerifyHashedPassword  See <see href="https://github.com/henkmollema/CryptoHelper">CryptoHelper</see>
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool VerifyHashedPassword(this string hashedPassword, string password) => CryptoHelper.Crypto.VerifyHashedPassword(hashedPassword, password);

    }
}
