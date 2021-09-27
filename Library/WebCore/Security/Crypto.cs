using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using WebInterface;

namespace WebCore.Security
{
    /// <summary>
    /// Cryptography implementation methods.
    /// </summary>
    public class Crypto : ICrypto
    {
        /// <summary>
        /// Crypto Instance.
        /// </summary>
        public static Crypto Instance
        {
            get => _crypto == null ? _crypto = new Crypto() { rsa = RSA() } : _crypto;
            set => _crypto = value;
        }
        internal static Crypto _crypto;

        //public static uint XXH(this byte[] bytes) => BitConverter.ToUInt32(new K4os.Hash.xxHash.XXH32().AsHashAlgorithm().ComputeHash(bytes), 0);
        /// <summary>
        /// XXH32
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public uint XXH32(byte[] bytes) => K4os.Hash.xxHash.XXH32.DigestOf(bytes.AsSpan());
        /// <summary>
        /// XXH32
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public uint XXH32(string text) { var bytes = Encoding.UTF8.GetBytes(text); return K4os.Hash.xxHash.XXH32.DigestOf(bytes, 0, bytes.Length); }
        /// <summary>
        /// XXH64
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public ulong XXH64(byte[] bytes) => K4os.Hash.xxHash.XXH64.DigestOf(bytes.AsSpan());
        /// <summary>
        /// XXH64
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public ulong XXH64(string text) { var bytes = Encoding.UTF8.GetBytes(text); return K4os.Hash.xxHash.XXH64.DigestOf(bytes, 0, bytes.Length); }
        /// <summary>
        /// Crc16
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public ushort Crc16(string text) => Crc16Algorithm.Crc16(Encoding.ASCII.GetBytes(text));
        /// <summary>
        /// Crc32
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public uint Crc32(string text) => Crc32Algorithm.Crc32(Encoding.ASCII.GetBytes(text));
        /// <summary>
        /// Crc32.ToString("x8")
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Crc32x8(string text) => Crc32(text).ToString("x8");
        /// <summary>
        /// Crc32.ToString("X8")
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Crc32X8(string text) => Crc32(text).ToString("X8");


        /// <summary>
        /// Encrypt an array with XOR.
        /// </summary>
        /// <param name="data">An unencrypted array.</param>
        /// <param name="keys">The encryption keys.</param>
        /// <returns>An encrypted array.</returns>
        public byte[] Xor(byte[] data, IReadOnlyList<byte> keys)
        {
            for (var i = 0; i < data.Length; i++) data[i] = (byte)(data[i] ^ keys[i]);
            return data;
        }


        /// <summary>
        /// 文本Base64编码 = btoa(encodeURIComponent(text))
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string ToBase64String(string text) => text.ToBase64String();
        /// <summary>
        /// 文本Base64解码 = decodeURIComponent(atob(text))
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string FromBase64String(string text) => text.FromBase64String();
        /// <summary>
        /// 判断文本为Base64编码
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool IsBase64(string text) => text.IsBase64();


        /// <summary>
        /// This is the .NET equivalent of crypto_auth.
        /// signs a message with a key.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/secret-key_authentication.html
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>The signature with 32 bytes</returns>
        public byte[] Sign(byte[] message, byte[] key) => Sodium.SecretKeyAuth.Sign(message, key);
        /// <summary>signs a message with a key.</summary>
        public byte[] SignHmacSha256(byte[] message, byte[] key) => Sodium.SecretKeyAuth.SignHmacSha256(message, key);
        /// <summary>signs a message with a key.</summary>
        public byte[] SignHmacSha512(byte[] message, byte[] key) => Sodium.SecretKeyAuth.SignHmacSha512(message, key);

        /// <summary>
        /// This is the .NET equivalent of crypto_auth_verify.
        /// verifies a message with a signature and a key signed by Sign().
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/secret-key_authentication.html
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signature">The signature must be 32 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns></returns>
        public bool Verify(byte[] message, byte[] signature, byte[] key) => Sodium.SecretKeyAuth.Verify(message, signature, key);
        /// <summary>verifies a message with a signature and a key signed.</summary>
        public bool VerifyHmacSha256(byte[] message, byte[] signature, byte[] key) => Sodium.SecretKeyAuth.VerifyHmacSha256(message, signature, key);
        /// <summary>verifies a message with a signature and a key signed.</summary>
        public bool VerifyHmacSha512(byte[] message, byte[] signature, byte[] key) => Sodium.SecretKeyAuth.VerifyHmacSha512(message, signature, key);


        /// <summary>
        /// This is the .NET equivalent of crypto_secretbox_easy.
        /// encrypts a message with a key and a nonce to keep it confidential.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/authenticated_encryption.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="nonce">The nonce must be 24 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>Encrypted data</returns>
        public byte[] Encrypt(byte[] message, byte[] nonce, byte[] key) => Sodium.SecretBox.Create(message, nonce, key);
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
        public byte[] Encrypt(byte[] message, byte[] nonce, byte[] key, byte[] additionalData) => Sodium.SecretAeadChaCha20Poly1305.Encrypt(message, nonce, key, additionalData);

        /// <summary>
        /// This is the .NET equivalent of crypto_secretbox_open_easy.
        /// decrypts a cipherText produced by Create(), with a key and a nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/authenticated_encryption.html
        /// </summary>
        /// <param name="cipher">Encrypted data</param>
        /// <param name="nonce">The nonce must be 24 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>Original data</returns>
        public byte[] Decrypt(byte[] cipher, byte[] nonce, byte[] key) => Sodium.SecretBox.Open(cipher, nonce, key);
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
        public byte[] Decrypt(byte[] cipher, byte[] nonce, byte[] key, byte[] additionalData) => Sodium.SecretAeadChaCha20Poly1305.Decrypt(cipher, nonce, key, additionalData);



        #region OneTime Auth Sign/Verify
        /// <summary>
        /// This is the .NET equivalent of crypto_onetimeauth.
        /// authenticates a message, with a key.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/poly1305.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>The signature data 16 bytes</returns>
        public byte[] OneTimeAuthSign(byte[] message, byte[] key) => Sodium.OneTimeAuth.Sign(message, key);
        /// <summary>
        /// This is the .NET equivalent of crypto_onetimeauth_verify.
        /// verifies a message, with a signature and a key.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/poly1305.html
        /// </summary>
        /// <param name="message">Original data</param>
        /// <param name="signature">The signature must be 16 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>returns true on success, otherwise false.</returns>
        public bool OneTimeAuthVerify(byte[] message, byte[] signature, byte[] key) => Sodium.OneTimeAuth.Verify(message, signature, key);
        /// <summary>
        /// Generate a 32 byte key, or GenerateKey(), or GenerateNonceBytes(32) to generate a 32 byte key.
        /// </summary>
        /// <returns></returns>
        public byte[] OneTimeAuthKey() => Sodium.OneTimeAuth.GenerateKey();
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
        public byte[] EncryptChaCha20(byte[] message, byte[] nonce, byte[] key) => Sodium.StreamEncryption.EncryptChaCha20(message, nonce, key);
        /// <summary>
        /// This is the .NET equivalent of crypto_stream_chacha20_xor.
        /// decrypts a message cipher using a secret key and a public nonce.
        /// https://bitbeans.gitbooks.io/libsodium-net/content/advanced/chacha20.html
        /// </summary>
        /// <param name="cipher">Encrypted data</param>
        /// <param name="nonce">The nonce must be 8 bytes</param>
        /// <param name="key">The key must be 32 bytes</param>
        /// <returns>Original data</returns>
        public byte[] DecryptChaCha20(byte[] cipher, byte[] nonce, byte[] key) => Sodium.StreamEncryption.DecryptChaCha20(cipher, nonce, key);
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
        public byte[] AES256GCMEncrypt(byte[] message, byte[] nonce, byte[] key, byte[] additionalData = null) => Sodium.SecretAeadAes.Encrypt(message, nonce, key, additionalData);
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
        public byte[] AES256GCMDecrypt(byte[] cipher, byte[] nonce, byte[] key, byte[] additionalData = null) => Sodium.SecretAeadAes.Decrypt(cipher, nonce, key, additionalData);
        /// <summary>
        /// Generate a 32 byte key, or GenerateNonceBytes(32) to generate a 32 byte key.
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateKey() => Sodium.StreamEncryption.GenerateKey();
        /// <summary>
        /// Generate a 24 byte nonce for encrypt: Sodium.SecretBox.Create(message, nonce, key), decrypt: Sodium.SecretBox.Open(ciphertext, nonce, key)
        /// https://bitbeans.gitbooks.io/libsodium-net/content/secret-key_cryptography/authenticated_encryption.html
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] GetRandomBytes(int count = 24) => Sodium.SodiumCore.GetRandomBytes(count);
        /// <summary>
        /// Generate 12 bytes for AES256GCMEncrypt(),AES256GCMDecrypt()
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateNonce() => Sodium.SecretAeadChaCha20Poly1305.GenerateNonce();
        /// <summary>
        /// Generate 8 bytes for EncryptChaCha20()
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateNonceBytes(int count = 8) => Sodium.SodiumCore.GetRandomBytes(count);
        /// <summary>
        /// Generate 8 bytes for EncryptChaCha20()
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateNonceChaCha20() => Sodium.StreamEncryption.GenerateNonceChaCha20();
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
        public byte[] AESEncrypt(byte[] password, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.ISO10126, int keySize = 256, int blockSize = 128) => AESencrypt(password, key, iv, mode, padding, keySize, blockSize);
        public static byte[] AESencrypt(byte[] password, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.ISO10126, int keySize = 256, int blockSize = 128)
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
        public byte[] AESDecrypt(byte[] hashedPassword, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.ISO10126, int keySize = 256, int blockSize = 128) => AESdecrypt(hashedPassword, key, iv, mode, padding, keySize, blockSize);
        public static byte[] AESdecrypt(byte[] hashedPassword, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.ISO10126, int keySize = 256, int blockSize = 128)
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
        public string AESEncrypt(string password, string key, string iv) => AESencrypt(password, key, iv);
        public static string AESencrypt(string password, string key, string iv)
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
        public string AESDecrypt(string hashedPassword, string key, string iv) => AESdecrypt(hashedPassword, key, iv);
        public static string AESdecrypt(string hashedPassword, string key, string iv)
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
        public string AESEncrypt(string password, string key) => AESencrypt(password, key);
        public static string AESencrypt(string password, string key)
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
        public string AESDecrypt(string hashedPassword, string key) => AESdecrypt(hashedPassword, key);
        public static string AESdecrypt(string hashedPassword, string key)
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

            if (!string.IsNullOrEmpty(keyContainerName) && !Platform.OS.IsPosix)
                rsa = new RSACryptoServiceProvider(keySize, new CspParameters() { KeyContainerName = keyContainerName });

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
        /// Generates and returns a random or rastgele bytes.
        /// </summary>
        /// <param name="length">Length of the bytes to be returned.</param>
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
        /// Generates and returns a random sequence of strings
        /// </summary>
        /// <param name="length">Length of the string to be returned.</param>
        /// <returns>Captcha string</returns>
        public string RandomString(int length)
        {
            if (length <= 0) throw new ArgumentException("length cannot be less than or equal to 0");
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var fillBuffer = new byte[4];
            int number;

            //CryptoService random byte generator for randomization
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                for (int i = 0; i < stringChars.Length; i++)
                {
                    randomNumberGenerator.GetBytes(fillBuffer);
                    number = Math.Abs(BitConverter.ToInt32(fillBuffer, 0)) % 60;
                    stringChars[i] = chars[number];
                }
            }

            return new string(stringChars);
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
                byte[] o = h.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(o);
                //var s = new StringBuilder();
                //foreach (var b in o) s.AppendFormat("{0:x2}", b);
                //return s.ToString();
            }
        }

        /// <summary>
        /// 随机哈希算法 > Hmac算法也是一种哈希算法，它可以利用MD5或SHA1等哈希算法。
        ///   不同的是，Hmac还需要一个密钥；只要密钥发生了变化，那么同样的输入也会得到不同的签名。
        ///   因此，可以把Hmac理解为用随机数“增强”的哈希算法。
        ///
        /// HS256 = HMACSHA256 (HMAC+SHA256) Encryption.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string HS256(string password, string key) => HMACSHA256(password, key);
        /// <summary>
        /// HMAC+SHA256
        /// </summary>
        public static string HMACSHA256(string password, string key)
        {
            using (var h = new HMACSHA256(Encoding.UTF8.GetBytes(key))) return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
        /// <summary>
        /// HS512 = HMACSHA512
        /// </summary>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string HS512(string password, string key) => HMACSHA512(password, key);
        /// <summary>
        /// HMAC+SHA512
        /// </summary>
        public static string HMACSHA512(string password, string key)
        {
            using (var h = new HMACSHA512(Encoding.UTF8.GetBytes(key))) return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
        /// <summary>
        /// HS1 = HMACSHA1
        /// </summary>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string HS1(string password, string key) => HMACSHA1(password, key);
        /// <summary>
        /// HMAC+SHA1
        /// </summary>
        public static string HMACSHA1(string password, string key)
        {
            using (var h = new HMACSHA1(Encoding.UTF8.GetBytes(key))) return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
        /// <summary>
        /// HMD5 = HMACMD5
        /// </summary>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string HMD5(string password, string key) => HMACMD5(password, key);
        /// <summary>
        /// HMAC+MD5
        /// </summary>
        public static string HMACMD5(string password, string key)
        {
            using (var h = new HMACMD5(Encoding.UTF8.GetBytes(key))) return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(password)));
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
}
