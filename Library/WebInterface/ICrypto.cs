using System.Collections.Generic;
using System.Security.Cryptography;

namespace WebInterface
{
    /// <summary>
    /// Cryptography methods.
    /// </summary>
    public interface ICrypto
    {
        uint XXH32(byte[] bytes);
        uint XXH32(string text);
        ulong XXH64(byte[] bytes);
        ulong XXH64(string text);
        ushort Crc16(string text);
        uint Crc32(string text);
        string Crc32x8(string text);
        string Crc32X8(string text);

        /// <summary>
        /// Encrypt an array with XOR.
        /// </summary>
        /// <param name="data">An unencrypted array.</param>
        /// <param name="keys">The encryption keys.</param>
        /// <returns>An encrypted array.</returns>
        byte[] Xor(byte[] data, IReadOnlyList<byte> keys);


        /// <summary>
        /// 文本Base64编码 = btoa(encodeURIComponent(text))
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string ToBase64String(string text);
        /// <summary>
        /// 文本Base64解码 = decodeURIComponent(atob(text))
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string FromBase64String(string text);
        /// <summary>
        /// 判断文本为Base64编码
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        bool IsBase64(string text);


        /// <summary>
        /// This is the .NET equivalent of crypto_auth.
        /// signs a message with a key.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/secret-key_authentication.html
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>The signature with 32 bytes</returns>
        byte[] Sign(byte[] message, byte[] key);
        /// <summary>signs a message with a key.</summary>
        byte[] SignHmacSha256(byte[] message, byte[] key);
        /// <summary>signs a message with a key.</summary>
        byte[] SignHmacSha512(byte[] message, byte[] key);

        /// <summary>
        /// This is the .NET equivalent of crypto_auth_verify.
        /// verifies a message with a signature and a key signed by Sign().
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/secret-key_authentication.html
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signature">The signature must be 32 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns></returns>
        bool Verify(byte[] message, byte[] signature, byte[] key);
        /// <summary>verifies a message with a signature and a key signed.</summary>
        bool VerifyHmacSha256(byte[] message, byte[] signature, byte[] key);
        /// <summary>verifies a message with a signature and a key signed.</summary>
        bool VerifyHmacSha512(byte[] message, byte[] signature, byte[] key);


        /// <summary>
        /// This is the .NET equivalent of crypto_secretbox_easy.
        /// encrypts a message with a key and a nonce to keep it confidential.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/authenticated_encryption.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="nonce">The nonce must be 24 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>Encrypted data</returns>
        byte[] Encrypt(byte[] message, byte[] nonce, byte[] key);
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
        byte[] Encrypt(byte[] message, byte[] nonce, byte[] key, byte[] additionalData);

        /// <summary>
        /// This is the .NET equivalent of crypto_secretbox_open_easy.
        /// decrypts a cipherText produced by Create(), with a key and a nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/authenticated_encryption.html
        /// </summary>
        /// <param name="cipher">Encrypted data</param>
        /// <param name="nonce">The nonce must be 24 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>Original data</returns>
        byte[] Decrypt(byte[] cipher, byte[] nonce, byte[] key);
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
        byte[] Decrypt(byte[] cipher, byte[] nonce, byte[] key, byte[] additionalData);



        #region OneTime Auth Sign/Verify
        /// <summary>
        /// This is the .NET equivalent of crypto_onetimeauth.
        /// authenticates a message, with a key.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/poly1305.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>The signature data 16 bytes</returns>
        byte[] OneTimeAuthSign(byte[] message, byte[] key);
        /// <summary>
        /// This is the .NET equivalent of crypto_onetimeauth_verify.
        /// verifies a message, with a signature and a key.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/poly1305.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="signature">The signature must be 16 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>returns true on success, otherwise false.</returns>
        bool OneTimeAuthVerify(byte[] message, byte[] signature, byte[] key);
        /// <summary>
        /// Generate a 32 byte key, or GenerateKey(), or GenerateNonceBytes(32) to generate a 32 byte key.
        /// </summary>
        /// <returns></returns>
        byte[] OneTimeAuthKey();
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
        byte[] EncryptChaCha20(byte[] message, byte[] nonce, byte[] key);
        /// <summary>
        /// This is the .NET equivalent of crypto_stream_chacha20_xor.
        /// decrypts a message cipher using a secret key and a public nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/chacha20.html
        /// </summary>
        /// <param name="cipher">Encrypted data</param>
        /// <param name="nonce">The nonce must be 8 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>Original data</returns>
        byte[] DecryptChaCha20(byte[] cipher, byte[] nonce, byte[] key);
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
        byte[] AES256GCMEncrypt(byte[] message, byte[] nonce, byte[] key, byte[] additionalData = null);
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
        byte[] AES256GCMDecrypt(byte[] cipher, byte[] nonce, byte[] key, byte[] additionalData = null);
        /// <summary>
        /// Generate a 32 byte key, or GenerateNonceBytes(32) to generate a 32 byte key.
        /// </summary>
        /// <returns></returns>
        byte[] GenerateKey();
        /// <summary>
        /// Generate a 24 byte nonce for encrypt: Sodium.SecretBox.Create(message, nonce, key), decrypt: Sodium.SecretBox.Open(ciphertext, nonce, key)
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/authenticated_encryption.html
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        byte[] GetRandomBytes(int count = 24);
        /// <summary>
        /// Generate 12 bytes for AES256GCMEncrypt(),AES256GCMDecrypt()
        /// </summary>
        /// <returns></returns>
        byte[] GenerateNonce();
        /// <summary>
        /// Generate 8 bytes for EncryptChaCha20()
        /// </summary>
        /// <returns></returns>
        byte[] GenerateNonceBytes(int count = 8);
        /// <summary>
        /// Generate 8 bytes for EncryptChaCha20()
        /// </summary>
        /// <returns></returns>
        byte[] GenerateNonceChaCha20();
        #endregion


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
        /// Generates and returns a random or rastgele bytes.
        /// </summary>
        /// <param name="length">Length of the bytes to be returned.</param>
        /// <returns></returns>
        byte[] RandomBytes(int length);
        /// <summary>
        /// Generates and returns a random sequence of strings
        /// </summary>
        /// <param name="length">Length of the string to be returned.</param>
        /// <returns>Captcha string</returns>
        string RandomString(int length);

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
