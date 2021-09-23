using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebFramework.Authentication.WeixinMiniProgram
{
    /// <summary></summary>
    public class WeixinLoginStateInfoStore : IWeixinLoginStateInfoStore
    {
        private readonly IDistributedCache _cache;
        private const string keyPrefix = "WeixinMiniProgram-";

        /// <summary></summary>
        public WeixinLoginStateInfoStore(IDistributedCache distributedCache)
        {
            _cache = distributedCache;
        }

        /// <summary></summary>
        public async Task<WeixinLoginSessionInfo> GetSessionInfo(string key)
        {
            var value = await _cache.GetAsync(key);
            if (value == null) return null;

            return JsonSerializer.Deserialize<WeixinLoginSessionInfo>(value);
        }

        /// <summary></summary>
        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        /// <summary></summary>
        public async Task RenewAsync(string key, WeixinLoginSessionInfo sessionInfo, WeixinLoginOptions currentOption)
        {
            await _cache.RemoveAsync(key);

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
            options.SetSlidingExpiration(currentOption.CacheExpiration);
            await _cache.SetAsync(key, CreateSesionBytes(sessionInfo), options);
        }

        /// <summary></summary>
        public async Task<string> StoreAsync(WeixinLoginSessionInfo sessionInfo, WeixinLoginOptions currentOption)
        {
            var key = keyPrefix + Guid.NewGuid().ToString();

            var options = new DistributedCacheEntryOptions();
            options.SetSlidingExpiration(currentOption.CacheExpiration);

            await _cache.SetAsync(key, CreateSesionBytes(sessionInfo), options);

            return key;
        }

        private byte[] CreateSesionBytes(WeixinLoginSessionInfo sessionInfo)
        {
            return JsonSerializer.SerializeToUtf8Bytes(sessionInfo, typeof(WeixinLoginSessionInfo));
        }
    }

    /// <summary>
    /// 保存微信服务端返回的Sessionkey等信息到缓存.
    /// </summary>
    public interface IWeixinLoginStateInfoStore
    {
        /// <summary>
        /// 保存<see cref="WeixinLoginSessionInfo"/>,并且返回所关联的Key。
        /// </summary>
        /// <param name="sessionInfo"><see cref="WeixinLoginSessionInfo"/></param>
        /// <param name="currentOption">当前的微信验证配置信息</param>
        /// <returns>与该seesionInfo所关联的Key信息</returns>
        Task<string> StoreAsync(WeixinLoginSessionInfo sessionInfo, WeixinLoginOptions currentOption);

        /// <summary>
        /// 刷新当前Key的所对应的信息。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sessionInfo"><see cref="WeixinLoginSessionInfo"/></param>
        /// <param name="currentOption">当前的微信验证配置信息</param>
        Task RenewAsync(string key, WeixinLoginSessionInfo sessionInfo, WeixinLoginOptions currentOption);

        /// <summary>
        /// 根据Key来移除缓存中的结果。
        /// </summary>
        /// <param name="key"></param>
        Task RemoveAsync(string key);

        /// <summary>
        /// 根据Key来获取对应的<see cref="WeixinLoginSessionInfo"/>。
        /// </summary>
        /// <param name="key"></param>
        Task<WeixinLoginSessionInfo> GetSessionInfo(string key);
    }

    /// <summary>
    /// 微信服务端返回的密匙信息.
    /// </summary>
    public class WeixinLoginSessionInfo
    {
        /// <summary>
        /// 用户唯一标识
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 会话密钥
        /// </summary>
        public string SessionKey { get; set; }

        public WeixinLoginSessionInfo(string openId, string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(openId))
                throw new ArgumentException($"{nameof(openId)} 不能为空!");

            if (string.IsNullOrWhiteSpace(sessionKey))
                throw new ArgumentException($"{nameof(sessionKey)} 不能为空!");

            OpenId = openId;
            SessionKey = sessionKey;
        }
    }
}
