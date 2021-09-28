using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Senparc.Weixin.MP.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using WebCore;

namespace WebFramework.Services.Weixin.MP
{
    /// <summary>
    /// 管理图文素材
    /// </summary>
    public static class Material
    {
        /// <summary>
        /// 获取图文素材
        /// </summary>
        public static T GetWeixinMPMaterial<T>(this IDistributedCache cache, string media_id, int cacheTime = 7200, string token = null, string appId = null, string appSecret = null)
        {
            var v = cache.GetString(media_id);
            T dto = v == null ? default : JsonConvert.DeserializeObject<T>(v);
            if (dto != null) return dto;

            if (appId == null) appId = Senparc.Weixin.Config.SenparcWeixinSetting.WeixinAppId;
            if (appSecret == null) appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WeixinAppSecret;

            if (string.IsNullOrEmpty(token))
                token = AccessTokenContainer.TryGetAccessToken(appId, appSecret);
            if (string.IsNullOrEmpty(token))
                throw new Exception("系统配置错误！");

            try
            {
                var url = new Uri(Senparc.Weixin.Config.ApiMpHost + $"/cgi-bin/material/get_material?access_token={token}");
                var result = url.JsonRequest(new { media_id });
                var results = new { news_item = new List<T>() };
                results = JsonConvert.DeserializeAnonymousType(result, results);
                dto = results.news_item.FirstOrDefault();
                if (dto == null) return dto;
                v = JsonConvert.SerializeObject(dto);
                cache.SetString(media_id, v, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(cacheTime) });
                return dto;
            }
            catch
            {
                return default;
            }
        }
    }
}
