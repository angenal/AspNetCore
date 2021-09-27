using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web;
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
        public static uint XXH32(this string text) { var bytes = Crypto.Encoding.Default.GetBytes(text); return K4os.Hash.xxHash.XXH32.DigestOf(bytes, 0, bytes.Length); }
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
        public static ulong XXH64(this string text) { var bytes = Crypto.Encoding.Default.GetBytes(text); return K4os.Hash.xxHash.XXH64.DigestOf(bytes, 0, bytes.Length); }
        /// <summary>
        /// Crc16
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ushort Crc16(this string text) => Crc16Algorithm.Crc16(Crypto.Encoding.Default.GetBytes(text));
        /// <summary>
        /// Crc32
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static uint Crc32(this string text) => Crc32Algorithm.Crc32(Crypto.Encoding.Default.GetBytes(text));
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

        /// <summary>
        /// Encrypt an array with XOR.
        /// </summary>
        /// <param name="data">An unencrypted array.</param>
        /// <param name="keys">The encryption keys.</param>
        /// <returns>An encrypted array.</returns>
        public static byte[] Xor(this byte[] data, IReadOnlyList<byte> keys)
        {
            for (var i = 0; i < data.Length; i++) data[i] = (byte)(data[i] ^ keys[i]);
            return data;
        }


        /// <summary>
        /// 文本Base64编码 = btoa(encodeURIComponent(text))
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToBase64String(this string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            return Convert.ToBase64String(Crypto.Encoding.Default.GetBytes(HttpUtility.UrlEncode(text)));
        }
        /// <summary>
        /// 文本Base64解码 = decodeURIComponent(atob(text))
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FromBase64String(this string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            return HttpUtility.UrlDecode(Crypto.Encoding.Default.GetString(Convert.FromBase64String(text)));
        }
        /// <summary>
        /// 判断文本为Base64编码
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsBase64(this string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length % 4 != 0) return false;
            int length = text.Length;
            if (text[length - 1] == 61) --length;
            if (text[length - 1] == 61) --length;
            for (int i = 0; i < length; ++i)
            {
                int c = text[i];
                if (c < 43 || c > 43 && c < 47 || c > 57 && c < 65 || c > 122) return false;
            }
            return true;
        }


        /// <summary>
        /// This is the .NET equivalent of crypto_auth.
        /// signs a message with a key.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/secret-key_authentication.html
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>The signature with 32 bytes</returns>
        public static byte[] Sign(this byte[] message, byte[] key) => Sodium.SecretKeyAuth.Sign(message, key);
        /// <summary>signs a message with a key.</summary>
        public static byte[] SignHmacSha256(this byte[] message, byte[] key) => Sodium.SecretKeyAuth.SignHmacSha256(message, key);
        /// <summary>signs a message with a key.</summary>
        public static byte[] SignHmacSha512(this byte[] message, byte[] key) => Sodium.SecretKeyAuth.SignHmacSha512(message, key);

        /// <summary>
        /// This is the .NET equivalent of crypto_auth_verify.
        /// verifies a message with a signature and a key signed by Sign().
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/secret-key_authentication.html
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signature">The signature must be 32 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns></returns>
        public static bool Verify(this byte[] message, byte[] signature, byte[] key) => Sodium.SecretKeyAuth.Verify(message, signature, key);
        /// <summary>verifies a message with a signature and a key signed.</summary>
        public static bool VerifyHmacSha256(this byte[] message, byte[] signature, byte[] key) => Sodium.SecretKeyAuth.VerifyHmacSha256(message, signature, key);
        /// <summary>verifies a message with a signature and a key signed.</summary>
        public static bool VerifyHmacSha512(this byte[] message, byte[] signature, byte[] key) => Sodium.SecretKeyAuth.VerifyHmacSha512(message, signature, key);


        /// <summary>
        /// This is the .NET equivalent of crypto_secretbox_easy.
        /// encrypts a message with a key and a nonce to keep it confidential.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/authenticated_encryption.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="nonce">The nonce must be 24 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>Encrypted data</returns>
        public static byte[] Encrypt(this byte[] message, byte[] nonce, byte[] key) => Sodium.SecretBox.Create(message, nonce, key);
        /// <summary>
        /// This is the .NET equivalent of crypto_aead_chacha20poly1305_encrypt.
        /// encrypts a message using a secret key and a public nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/aead.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="nonce">The nonce must be 8 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <param name="additionalData">The additionalData may be null, or between 0 and 16 bytes</param>
        /// <returns>Encrypted data</returns>
        public static byte[] Encrypt(this byte[] message, byte[] nonce, byte[] key, byte[] additionalData) => Sodium.SecretAeadChaCha20Poly1305.Encrypt(message, nonce, key, additionalData);

        /// <summary>
        /// This is the .NET equivalent of crypto_secretbox_open_easy.
        /// decrypts a cipherText produced by Create(), with a key and a nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/authenticated_encryption.html
        /// </summary>
        /// <param name="cipher">Encrypted data</param>
        /// <param name="nonce">The nonce must be 24 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>Original data</returns>
        public static byte[] Decrypt(this byte[] cipher, byte[] nonce, byte[] key) => Sodium.SecretBox.Open(cipher, nonce, key);
        /// <summary>
        /// This is the .NET equivalent of crypto_aead_chacha20poly1305_decrypt.
        /// decrypts a message cipher using a secret key and a public nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/authenticated_encryption.html
        /// </summary>
        /// <param name="cipher">Encrypted data</param>
        /// <param name="nonce">The nonce must be 8 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <param name="additionalData">The additionalData may be null, or between 0 and 16 bytes</param>
        /// <returns>Original data</returns>
        public static byte[] Decrypt(this byte[] cipher, byte[] nonce, byte[] key, byte[] additionalData) => Sodium.SecretAeadChaCha20Poly1305.Decrypt(cipher, nonce, key, additionalData);



        #region OneTime Auth Sign/Verify
        /// <summary>
        /// This is the .NET equivalent of crypto_onetimeauth.
        /// authenticates a message, with a key.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/poly1305.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>The signature data 16 bytes</returns>
        public static byte[] OneTimeAuthSign(this byte[] message, byte[] key) => Sodium.OneTimeAuth.Sign(message, key);
        /// <summary>
        /// This is the .NET equivalent of crypto_onetimeauth_verify.
        /// verifies a message, with a signature and a key.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/poly1305.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="signature">The signature must be 16 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>returns true on success, otherwise false.</returns>
        public static bool OneTimeAuthVerify(this byte[] message, byte[] signature, byte[] key) => Sodium.OneTimeAuth.Verify(message, signature, key);
        /// <summary>
        /// Generate a 32 byte key, or GenerateKey(), or GenerateNonceBytes(32) to generate a 32 byte key.
        /// </summary>
        /// <returns></returns>
        public static byte[] OneTimeAuthKey() => Sodium.OneTimeAuth.GenerateKey();
        #endregion

        #region chacha20-ietf-poly1305
        /// <summary>
        /// This is the .NET equivalent of crypto_stream_chacha20_xor.
        /// encrypts a message using a secret key and a public nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/chacha20.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="nonce">The nonce must be 8 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>Encrypted data</returns>
        public static byte[] EncryptChaCha20(this byte[] message, byte[] nonce, byte[] key) => Sodium.StreamEncryption.EncryptChaCha20(message, nonce, key);
        /// <summary>
        /// This is the .NET equivalent of crypto_stream_chacha20_xor.
        /// decrypts a message cipher using a secret key and a public nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/chacha20.html
        /// </summary>
        /// <param name="cipher">Encrypted data</param>
        /// <param name="nonce">The nonce must be 8 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>Original data</returns>
        public static byte[] DecryptChaCha20(this byte[] cipher, byte[] nonce, byte[] key) => Sodium.StreamEncryption.DecryptChaCha20(cipher, nonce, key);
        #endregion

        #region aes-256-gcm
        /// <summary>
        /// This is the .NET equivalent of crypto_aead_aes256gcm_encrypt,
        /// encrypts a message using a secret key and a public nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/aes256_gcm.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="nonce">The nonce must be 12 bytes, or use Sodium.SecretAead.GenerateNonce()</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <param name="additionalData">The additionalData between 0 and 16 bytes</param>
        /// <returns>Encrypted data</returns>
        public static byte[] AES256GCMEncrypt(this byte[] message, byte[] nonce, byte[] key, byte[] additionalData = null) => Sodium.SecretAeadAes.Encrypt(message, nonce, key, additionalData);
        /// <summary>
        /// This is the .NET equivalent of crypto_aead_aes256gcm_decrypt,
        /// decrypts a message cipher using a secret key and a public nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/aes256_gcm.html
        /// </summary>
        /// <param name="cipher">Encrypted data</param>
        /// <param name="nonce">The nonce must be 12 bytes, or use GenerateNonce()</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <param name="additionalData">The additionalData between 0 and 16 bytes</param>
        /// <returns>Original data</returns>
        public static byte[] AES256GCMDecrypt(this byte[] cipher, byte[] nonce, byte[] key, byte[] additionalData = null) => Sodium.SecretAeadAes.Decrypt(cipher, nonce, key, additionalData);
        /// <summary>
        /// Generate a 32 byte key, or GenerateNonceBytes(32) to generate a 32 byte key.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateKey() => Sodium.StreamEncryption.GenerateKey();
        /// <summary>
        /// Generate a 24 byte nonce for encrypt: Sodium.SecretBox.Create(message, nonce, key), decrypt: Sodium.SecretBox.Open(ciphertext, nonce, key)
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/authenticated_encryption.html
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] GetRandomBytes(int count = 24) => Sodium.SodiumCore.GetRandomBytes(count);
        /// <summary>
        /// Generate 12 bytes for AES256GCMEncrypt(),AES256GCMDecrypt()
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateNonce() => Sodium.SecretAeadChaCha20Poly1305.GenerateNonce();
        /// <summary>
        /// Generate 8 bytes for EncryptChaCha20()
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateNonceBytes(int count = 8) => Sodium.SodiumCore.GetRandomBytes(count);
        /// <summary>
        /// Generate 8 bytes for EncryptChaCha20()
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateNonceChaCha20() => Sodium.StreamEncryption.GenerateNonceChaCha20();
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

        /// <summary>
        /// AESEncrypt + CBC + Pkcs7
        /// </summary>
        public static byte[] AESCBCPkcs7Encrypt(this string plainText, byte[] key, byte[] iv) => Crypto.Instance.AESCBCPkcs7Encrypt(plainText, key, iv);
        /// <summary>
        /// AESDecrypt + CBC + Pkcs7
        /// </summary>
        public static string AESCBCPkcs7Decrypt(this byte[] cipherText, byte[] key, byte[] iv) => Crypto.Instance.AESCBCPkcs7Decrypt(cipherText, key, iv);
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
