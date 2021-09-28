using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Containers;

namespace WebFramework.Weixins.MP
{
    /// <summary>
    /// 管理微信用户
    /// </summary>
    public static class User
    {
        /// <summary>
        /// 用户是否关注了公众号
        /// </summary>
        /// <param name="openid">用户的openid</param>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static bool IsSubscribe(string openid, string appid = null)
        {
            try
            {
                if (appid == null) appid = Senparc.Weixin.Config.SenparcWeixinSetting.WeixinAppId;

                var access = AccessTokenContainer.GetAccessTokenResult(appid);
                var usr = UserApi.Info(access.access_token, openid);
                return usr != null && usr.subscribe != 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
