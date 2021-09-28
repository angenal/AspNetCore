using System;
using System.Collections.Generic;
using System.IO;

namespace WebFramework.Services.Weixin.Data
{
    /// <summary>
    /// 公众号
    /// </summary>
    public static class MP
    {
        /// <summary>
        /// 根目录
        /// </summary>
        public static string RootPath = Path.Combine(Environment.CurrentDirectory, "App_Data", "Weixin", nameof(MP));

        /// <summary>
        /// 被关注回复
        /// </summary>
        public static string SubscribeRequestReply;
        public static bool SetSubscribeRequestReply(string s)
        {
            string file = Path.Combine(RootPath, "SubscribeRequestReply.txt");
            File.WriteAllText(file, s);
            SubscribeRequestReply = s;
            return true;
        }

        /// <summary>
        /// 文本消息的请求回复内容 /根目录/*.json
        /// </summary>
        public static Dictionary<string, DTO.RequestReplyDTO> TextRequestReply = new Dictionary<string, DTO.RequestReplyDTO>();
        /// <summary></summary>
        public static void InitTextRequestReply(string file = "*.json")
        {
            foreach (string fileName in Directory.GetFiles(Path.Combine(RootPath, "TextRequestReply"), file))
            {
                string s = File.ReadAllText(fileName);
                var dto = Newtonsoft.Json.JsonConvert.DeserializeObject<DTO.RequestReplyDTO>(s);
                if (string.IsNullOrEmpty(dto.keyword)) continue;
                lock (TextRequestReply)
                {
                    if (TextRequestReply.ContainsKey(dto.keyword)) TextRequestReply[dto.keyword] = dto;
                    else TextRequestReply.Add(dto.keyword, dto);
                }
            }
        }
        /// <summary></summary>
        public static string SetTextRequestReply(DTO.RequestReplyDTO dto)
        {
            string file = dto.keyword + ".json";
            var s = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine($"{RootPath}/TextRequestReply", file), s);
            return file;
        }

        /// <summary>
        /// 保存用户关注状态
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="subscribe">用户是否订阅该公众号标识，值为0时，代表此用户没有关注该公众号，拉取不到其余信息。</param>
        /// <param name="appid"></param>
        public static void SaveSubscribeStatus(string openid, int subscribe, string appid = null)
        {
            if (appid == null) appid = Senparc.Weixin.Config.SenparcWeixinSetting.WeixinAppId;
            //LazyCache.App.SetRedisCache(CacheKSubscribeStatus + appid + openid, subscribe.ToString(), CacheKexpiration);
        }
    }
}
