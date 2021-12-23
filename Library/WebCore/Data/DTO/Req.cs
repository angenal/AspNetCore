using System;

namespace WebCore.Data.DTO
{
    public static class Req
    {
        /// <summary>
        /// 获取 时间戳(Utc)
        /// </summary>
        /// <returns></returns>
        public static long Timestamp() => DateTime.UtcNow.ToJavaScriptTicks();
        /// <summary>
        /// 获取 随机数值
        /// </summary>
        /// <returns></returns>
        public static string Nonce() => new Random((int)DateTime.Now.Ticks).Next(1000, 9999).ToString();
        /// <summary>
        /// 获取 签名 HMAC+MD5
        /// </summary>
        /// <param name="id">appid|userid</param>
        /// <param name="timestamp">时间戳(Utc)</param>
        /// <param name="nonce">随机数值</param>
        /// <param name="secretKey">凭证密钥Key</param>
        /// <returns></returns>
        public static string Sinature_HMACMD5(string id, string timestamp, string nonce, string secretKey) => Security.Crypto.HMACMD5($"{id}-{timestamp}-{nonce}", secretKey);
        /// <summary>
        /// 获取 签名 HMAC+SHA1
        /// </summary>
        /// <param name="id">appid|userid</param>
        /// <param name="timestamp">时间戳(Utc)</param>
        /// <param name="nonce">随机数值</param>
        /// <param name="secretKey">凭证密钥Key</param>
        /// <returns></returns>
        public static string Sinature_HMACSHA1(string id, string timestamp, string nonce, string secretKey) => Security.Crypto.HMACSHA1($"{id}-{timestamp}-{nonce}", secretKey);
        /// <summary>
        /// 获取 签名 HMAC+SHA256
        /// </summary>
        /// <param name="id">appid|userid</param>
        /// <param name="timestamp">时间戳(Utc)</param>
        /// <param name="nonce">随机数值</param>
        /// <param name="secretKey">凭证密钥Key</param>
        /// <returns></returns>
        public static string Sinature_HMACSHA256(string id, string timestamp, string nonce, string secretKey) => Security.Crypto.HMACSHA256($"{id}-{timestamp}-{nonce}", secretKey);
        /// <summary>
        /// 获取 签名 HMAC+SHA512
        /// </summary>
        /// <param name="id">appid|userid</param>
        /// <param name="timestamp">时间戳(Utc)</param>
        /// <param name="nonce">随机数值</param>
        /// <param name="secretKey">凭证密钥Key</param>
        /// <returns></returns>
        public static string Sinature_HMACSHA512(string id, string timestamp, string nonce, string secretKey) => Security.Crypto.HMACSHA512($"{id}-{timestamp}-{nonce}", secretKey);
    }
    /// <summary>
    /// 客户端请求头信息
    /// </summary>
    public class ReqInfo
    {
        /// <summary>
        /// 时间戳(Utc) ConvertDateTimeToJavaScriptTicks 转换Utc时间为JavaScript时间戳
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// 随机数值 number once
        /// </summary>
        public string nonce { get; set; }
        /// <summary>
        /// 签名[MD5,SHA,HMAC...]
        /// </summary>
        public string sinature { get; set; }
    }
    /// <summary>
    /// App客户端请求头信息
    /// </summary>
    public class ReqAppInfo : ReqInfo, IAppInfo
    {
        public string appid { get; set; }
        public string secret { get; set; }
    }
    /// <summary>
    /// User客户端请求头信息
    /// </summary>
    public class ReqUserInfo : ReqInfo, IUserInfo
    {
        public string userid { get; set; }
        public string secret { get; set; }
    }
}
