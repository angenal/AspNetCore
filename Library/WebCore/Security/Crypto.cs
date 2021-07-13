using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebCore.Security
{
    /// <summary>
    /// Cryptography implementation methods.
    /// </summary>
    public class Crypto : ICrypto
    {
        /// <summary>
        /// Configuration AES Encryption in appsettings.json
        /// </summary>
        public const string AesAppSettings = "AES";

        /// <summary>
        /// AES Encryption settings.
        /// </summary>
        public static AesSettings AesSettings = new AesSettings();

        /// <summary>
        /// Crypto Instance.
        /// </summary>
        public static Crypto Instance
        {
            get => _crypto == null ? _crypto = new Crypto() { rsa = RSA() } : _crypto;
            set => _crypto = value;
        }
        internal static Crypto _crypto;


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
        public byte[] AESEncrypt(byte[] password, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.ISO10126, int keySize = 256, int blockSize = 128)
        {
            byte[] cikti;
            using (MemoryStream ms = new MemoryStream())
            using (Rijndael aes = Rijndael.Create())
            {
                aes.BlockSize = blockSize;
                aes.KeySize = keySize;
                aes.Mode = mode;
                aes.Padding = padding;
                aes.Key = key;  // GetBytes(aes.KeySize / 8)
                aes.IV = iv;    // GetBytes(aes.BlockSize / 8)
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(password, 0, password.Length);
                    cs.FlushFinalBlock();
                }
                cikti = ms.ToArray();
            }
            return cikti;
        }
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
        public byte[] AESDecrypt(byte[] hashedPassword, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.ISO10126, int keySize = 256, int blockSize = 128)
        {
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            using (Rijndael aes = Rijndael.Create())
            {
                aes.BlockSize = blockSize;
                aes.KeySize = keySize;
                aes.Mode = mode;
                aes.Padding = padding;
                aes.Key = key;  // GetBytes(aes.KeySize / 8)
                aes.IV = iv;    // GetBytes(aes.BlockSize / 8)
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(hashedPassword, 0, hashedPassword.Length);
                    cs.FlushFinalBlock();
                }
                bytes = ms.ToArray();
            }
            return bytes;
        }
        #endregion

        #region AES + CBC
        /// <summary>
        /// AESEncrypt + CBC
        /// </summary>
        public string AESEncrypt(string password, string key, string iv)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            SymmetricAlgorithm des = Aes.Create();
            des.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
            des.IV = Encoding.UTF8.GetBytes(iv);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputBytes, 0, inputBytes.Length);
                    cs.FlushFinalBlock();
                    byte[] outputBytes = ms.ToArray();
                    return Convert.ToBase64String(outputBytes, 0, outputBytes.Length);
                }
            }
        }
        /// <summary>
        /// AESDecrypt + CBC
        /// </summary>
        public string AESDecrypt(string hashedPassword, string key, string iv)
        {
            SymmetricAlgorithm des = Aes.Create();
            des.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
            des.IV = Encoding.UTF8.GetBytes(iv);
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(hashedPassword));
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
            try
            {
                int readBytes;
                byte[] buffer = new byte[1024];
                MemoryStream stream = new MemoryStream();
                while ((readBytes = cs.Read(buffer, 0, buffer.Length)) > 0) stream.Write(buffer, 0, readBytes);
                byte[] outputBytes = stream.ToArray();
                return Encoding.UTF8.GetString(outputBytes);
            }
            catch (Exception)
            {
                return string.Empty;
            }
            finally
            {
                cs?.Dispose();
                ms?.Dispose();
            }
        }
        #endregion

        #region AES + CEB
        /// <summary>
        /// AESEncrypt + CEB
        /// </summary>
        public string AESEncrypt(string password, string key)
        {
            if (string.IsNullOrEmpty(password)) return null;
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] outputBytes = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key.PadRight(32)),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            }.CreateEncryptor().TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            return Convert.ToBase64String(outputBytes, 0, outputBytes.Length);
        }
        /// <summary>
        /// AESDecrypt + CEB
        /// </summary>
        public string AESDecrypt(string hashedPassword, string key)
        {
            byte[] bKey = new byte[32], encryptedBytes = Convert.FromBase64String(hashedPassword);
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);
            SymmetricAlgorithm aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.Key = bKey;
            MemoryStream ms = new MemoryStream(encryptedBytes);
            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            try
            {
                byte[] buffer = new byte[encryptedBytes.Length + 32];
                int readBytes = cs.Read(buffer, 0, encryptedBytes.Length + 32);
                byte[] outputBytes = new byte[readBytes];
                Array.Copy(buffer, 0, outputBytes, 0, readBytes);
                return Encoding.UTF8.GetString(outputBytes);
            }
            catch (Exception)
            {
                return string.Empty;
            }
            finally
            {
                cs?.Dispose();
                ms?.Dispose();
            }
        }

        #endregion


        #region RSA
        /// <summary>
        /// RSA Instance.
        /// </summary>
        internal RSACryptoServiceProvider rsa;
        /// <summary>
        /// RSA Private Key.
        /// </summary>
        public string RSAPrivateKey => rsa.ToXmlString(true);
        /// <summary>
        /// RSA Public Key.
        /// </summary>
        public string RSAPublicKey => rsa.ToXmlString(false);
        /// <summary>
        /// New RSA Instance.
        /// </summary>
        /// <param name="keySize"></param>
        /// <param name="persistKeyInCsp"></param>
        /// <param name="keyContainerName"></param>
        /// <returns></returns>
        public RSACryptoServiceProvider NewRSA(int keySize = 1024, bool persistKeyInCsp = false, string keyContainerName = null) => RSA(keySize, persistKeyInCsp, keyContainerName);
        /// <summary>
        /// RSA Instance Creator.
        /// </summary>
        /// <param name="keySize"></param>
        /// <param name="persistKeyInCsp"></param>
        /// <param name="keyContainerName"></param>
        /// <returns></returns>
        public static RSACryptoServiceProvider RSA(int keySize = 1024, bool persistKeyInCsp = false, string keyContainerName = null)
        {
            if (!IsRSAKeySize(keySize))
                throw new ArgumentException("RSA KeySize unverified", nameof(keySize));

            var rsa = new RSACryptoServiceProvider(keySize);

            //if (!string.IsNullOrEmpty(keyContainerName) && Environment.OSVersion.Platform < PlatformID.Unix)
            //    rsa = new RSACryptoServiceProvider(keySize, new CspParameters() { KeyContainerName = keyContainerName });

            rsa.PersistKeyInCsp = persistKeyInCsp;
            return rsa;
        }
        /// <summary>
        /// Get encrypted data with RSA
        /// </summary>
        /// <param name="password">Original data</param>
        /// <param name="optimalAsymmetricEncryptionPadding"></param>
        /// <returns>Encrypted data</returns>
        public byte[] RSAEncrypt(byte[] password, bool optimalAsymmetricEncryptionPadding = true) => rsa.Encrypt(password, optimalAsymmetricEncryptionPadding);
        /// <summary>
        /// Get decrypted data with RSA
        /// </summary>
        /// <param name="hashedPassword">Encrypted data</param>
        /// <param name="optimalAsymmetricEncryptionPadding"></param>
        /// <returns>Original data</returns>
        public byte[] RSADecrypt(byte[] hashedPassword, bool optimalAsymmetricEncryptionPadding = true) => rsa.Decrypt(hashedPassword, optimalAsymmetricEncryptionPadding);
        /// <summary>
        /// Output RSA PrivateKey (for RSADecrypt) and PublicKey (for RSAEncrypt)
        /// </summary>
        public string ToRSAXmlString(bool privateKey) => rsa.ToXmlString(privateKey);
        /// <summary>
        /// Input RSA PrivateKey (for RSADecrypt) and PublicKey (for RSAEncrypt)
        /// </summary>
        public void FromRSAXmlString(string key = null) => rsa.FromXmlString(key ?? RSAPrivateKey);
        /// <summary>
        /// Get Specify maximum data length to encrypt RSA
        /// </summary>
        public static int MaxRSAKeySize(int keySize, bool asymmetricEncryptionPadding = true) => asymmetricEncryptionPadding ? ((keySize - 384) / 8) + 7 : ((keySize - 384) / 8) + 37;
        /// <summary>
        /// Check RSA Key Size
        /// </summary>
        public static bool IsRSAKeySize(int keySize) => keySize >= 384 && keySize <= 16384 && keySize % 8 == 0;
        #endregion

        #region RSA + AES
        /// <summary>
        /// Get encrypted data with ( RSA + AES )
        /// </summary>
        /// <param name="password">Original data</param>
        /// <param name="aesKeySize"></param>
        /// <param name="aesBlockSize"></param>
        /// <returns>Encrypted data</returns>
        public byte[] RSA2Encrypt(byte[] password, int aesKeySize = 256, int aesBlockSize = 128)
        {
            if (password == null || password.Length == 0)
                throw new ArgumentException("password is empty", nameof(password));

            byte[] key = RandomBytes(aesKeySize / 8), iv = RandomBytes(aesBlockSize / 8);

            byte[] pwdCrypt = AESEncrypt(password, key, iv);
            byte[] keyCrypt = RSAEncrypt(key), ivCrypt = RSAEncrypt(iv);
            byte[] dst = new byte[pwdCrypt.Length + keyCrypt.Length + ivCrypt.Length];

            Buffer.BlockCopy(keyCrypt, 0, dst, 0, keyCrypt.Length);
            Buffer.BlockCopy(ivCrypt, 0, dst, keyCrypt.Length, ivCrypt.Length);
            Buffer.BlockCopy(pwdCrypt, 0, dst, keyCrypt.Length + ivCrypt.Length, pwdCrypt.Length);

            return dst;
        }
        /// <summary>
        /// Get decrypted data with ( RSA + AES )
        /// </summary>
        /// <param name="hashedPassword">Encrypted data</param>
        /// <returns>Original data</returns>
        public byte[] RSA2Decrypt(byte[] hashedPassword)
        {
            if (hashedPassword == null || hashedPassword.Length == 0)
                throw new ArgumentException("hashedPassword is empty", nameof(hashedPassword));

            byte[] keyCrypt = new byte[rsa.KeySize >> 3], ivCrypt = new byte[rsa.KeySize >> 3];
            byte[] pwdCrypt = new byte[hashedPassword.Length - keyCrypt.Length - ivCrypt.Length];

            Buffer.BlockCopy(hashedPassword, 0, keyCrypt, 0, keyCrypt.Length);
            Buffer.BlockCopy(hashedPassword, keyCrypt.Length, ivCrypt, 0, ivCrypt.Length);
            Buffer.BlockCopy(hashedPassword, keyCrypt.Length + ivCrypt.Length, pwdCrypt, 0, pwdCrypt.Length);

            byte[] key = RSADecrypt(keyCrypt), iv = RSADecrypt(ivCrypt);

            return AESDecrypt(pwdCrypt, key, iv);
        }
        #endregion


        /// <summary>
        /// Get random or rastgele bytes.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] RandomBytes(int length)
        {
            byte[] bytes;
            using (RNGCryptoServiceProvider rastgeleSayiOlustur = new RNGCryptoServiceProvider())
            {
                bytes = new byte[length];
                rastgeleSayiOlustur.GetBytes(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// Md5
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Md5(string password) => BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "").ToLower();

        /// <summary>
        /// Sha1
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Sha1(string password)
        {
            using (var h = SHA1.Create())
            {
                var s = new StringBuilder();
                byte[] o = h.ComputeHash(Encoding.UTF8.GetBytes(password));
                foreach (var b in o) s.AppendFormat("{0:x2}", b);
                return s.ToString();
            }
        }

        /// <summary>
        /// HS256
        /// </summary>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string HS256(string password, string key)
        {
            using (var h = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var s = new StringBuilder();
                byte[] o = h.ComputeHash(Encoding.UTF8.GetBytes(password));
                foreach (var b in o) s.AppendFormat("{0:x2}", b);
                return s.ToString();
            }
        }

        /// <summary>
        /// HashPassword  See <see href="https://github.com/henkmollema/CryptoHelper">CryptoHelper</see>
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string HashPassword(string password) => CryptoHelper.Crypto.HashPassword(password);

        /// <summary>
        /// VerifyHashedPassword  See <see href="https://github.com/henkmollema/CryptoHelper">CryptoHelper</see>
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool VerifyHashedPassword(string hashedPassword, string password) => CryptoHelper.Crypto.VerifyHashedPassword(hashedPassword, password);
    }

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
        /// HS256 (HMAC+SHA256) Encryption.
        /// </summary>
        /// <param name="password">The password to generate a hash value for.</param>
        /// <param name="key"></param>
        /// <returns></returns>
        string HS256(string password, string key);

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
