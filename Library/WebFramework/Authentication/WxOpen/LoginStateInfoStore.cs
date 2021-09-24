using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication.WxOpen
{
    /// <summary>
    /// 保存微信服务端返回的OpenId,Sessionkey等信息到缓存.
    /// </summary>
    public class WxOpenLoginStateInfoStore : IWxOpenLoginStateInfoStore
    {
        private readonly IDistributedCache _cache;
        private const string keyPrefix = "WxOpen-";

        /// <summary></summary>
        public WxOpenLoginStateInfoStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary></summary>
        public async Task<WxOpenLoginSessionInfo> GetSessionInfo(string key)
        {
            var value = await _cache.GetAsync(key);
            if (value == null) return null;

            return JsonSerializer.Deserialize<WxOpenLoginSessionInfo>(value);
        }

        /// <summary></summary>
        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        /// <summary></summary>
        public async Task RenewAsync(string key, WxOpenLoginSessionInfo sessionInfo, WxOpenLoginOptions currentOption)
        {
            await _cache.RemoveAsync(key);

            var options = new DistributedCacheEntryOptions();
            options.SetSlidingExpiration(currentOption.CacheExpiration);
            await _cache.SetAsync(key, CreateSesionBytes(sessionInfo), options);
        }

        /// <summary></summary>
        public async Task<string> StoreAsync(WxOpenLoginSessionInfo sessionInfo, WxOpenLoginOptions currentOption)
        {
            var key = keyPrefix + Guid.NewGuid().ToString("N");

            var options = new DistributedCacheEntryOptions();
            options.SetSlidingExpiration(currentOption.CacheExpiration);

            await _cache.SetAsync(key, CreateSesionBytes(sessionInfo), options);

            return key;
        }

        private byte[] CreateSesionBytes(WxOpenLoginSessionInfo sessionInfo)
        {
            return JsonSerializer.SerializeToUtf8Bytes(sessionInfo, typeof(WxOpenLoginSessionInfo));
        }
    }

    /// <summary>
    /// 保存微信服务端返回的Sessionkey等信息到缓存.
    /// </summary>
    public interface IWxOpenLoginStateInfoStore
    {
        /// <summary>
        /// 保存<see cref="WxOpenLoginSessionInfo"/>,并且返回所关联的Key。
        /// </summary>
        /// <param name="sessionInfo"><see cref="WxOpenLoginSessionInfo"/></param>
        /// <param name="currentOption">当前的微信验证配置信息</param>
        /// <returns>与该seesionInfo所关联的Key信息</returns>
        Task<string> StoreAsync(WxOpenLoginSessionInfo sessionInfo, WxOpenLoginOptions currentOption);

        /// <summary>
        /// 刷新当前Key的所对应的信息。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sessionInfo"><see cref="WxOpenLoginSessionInfo"/></param>
        /// <param name="currentOption">当前的微信验证配置信息</param>
        Task RenewAsync(string key, WxOpenLoginSessionInfo sessionInfo, WxOpenLoginOptions currentOption);

        /// <summary>
        /// 根据Key来移除缓存中的结果。
        /// </summary>
        /// <param name="key"></param>
        Task RemoveAsync(string key);

        /// <summary>
        /// 根据Key来获取对应的<see cref="WxOpenLoginSessionInfo"/>。
        /// </summary>
        /// <param name="key"></param>
        Task<WxOpenLoginSessionInfo> GetSessionInfo(string key);
    }

    /// <summary>
    /// 微信服务端返回的密匙信息.
    /// </summary>
    public class WxOpenLoginSessionInfo
    {
        /// <summary>
        /// 用户唯一标识
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 会话密钥
        /// </summary>
        public string SessionKey { get; set; }

        public WxOpenLoginSessionInfo(string openId, string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(openId))
                throw new ArgumentException($"微信小程序 {nameof(openId)} 不能为空!");

            if (string.IsNullOrWhiteSpace(sessionKey))
                throw new ArgumentException($"微信小程序 {nameof(sessionKey)} 不能为空!");

            OpenId = openId;
            SessionKey = sessionKey;
        }
    }
}
