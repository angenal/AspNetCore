using Senparc.Weixin.Containers;
using Senparc.Weixin.MP.Containers;
using System;

namespace WebFramework.Services.Weixin.MP
{
    /// <summary>
    /// 管理AccessToken
    /// </summary>
    public class AccessToken
    {
        public static DTO.AccessTokenInfo Get(string appId = null, bool getNewToken = false)
        {
            if (appId == null) appId = Senparc.Weixin.Config.SenparcWeixinSetting.WeixinAppId;

            var access = AccessTokenContainer.GetAccessTokenResult(appId);
            var token = BaseContainer<AccessTokenBag>.TryGetItem(appId);

            if (DateTime.Now > token.AccessTokenExpireTime)
                getNewToken = true;

            if (getNewToken)
            {
                access = AccessTokenContainer.GetAccessTokenResult(appId, true);
                token = BaseContainer<AccessTokenBag>.TryGetItem(appId);
            }

            return new DTO.AccessTokenInfo
            {
                Token = access.access_token,
                Expire = token.AccessTokenExpireTime.LocalDateTime
            };
        }
    }
}
