using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.AdvancedAPIs.User;
using Senparc.Weixin.MP.Entities.Request;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WebCore;
using WebFramework;
using WebFramework.Data;
using WebFramework.Weixins.Data;
using WebFramework.Weixins.MessageHandlers;
using WebFramework.Weixins.MessageHandlers.CustomMessageHandlers;
using WebInterface;

namespace WebControllers.Controllers
{
    /// <summary>
    /// 微信公众号
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class WxController : ApiController
    {
        private readonly SenparcWeixinSetting setting;
        private readonly IRedisCachingProvider redis;
        private static readonly TimeSpan redisCacheTime = TimeSpan.FromDays(30);
        private readonly ILiteDb liteDb;
        private readonly ICrypto crypto;
        private readonly IJwtGenerator jwtToken;

        /// <summary></summary>
        public WxController(IOptions<SenparcWeixinSetting> setting, IEasyCachingProviderFactory cacheFactory, ILiteDb liteDb, ICrypto crypto, IJwtGenerator jwtToken)
        {
            this.setting = setting.Value;
            redis = cacheFactory.GetRedisProvider(EasyCachingConstValue.DefaultRedisName);
            this.liteDb = liteDb;
            this.crypto = crypto;
            this.jwtToken = jwtToken;
        }

        /// <summary>
        /// [配置]微信后台验证地址“接口配置信息”的Url填写如 https://YourDomainName/Wx
        /// </summary>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult Get([FromQuery] PostModel postModel, string echostr)
        {
            if (CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, setting.Token))
                return Content(echostr); //返回随机字符串则表示验证通过
            return Content("如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
        }

        /// <summary>
        /// [配置]用户发送消息后，微信平台自动Post一个请求到这里，并等待响应XML
        /// </summary>
        [HttpPost]
        [Produces(Produces.XML)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Post(PostModel postModel)
        {
            string error = "<root><errcode>{0}</errcode><errmsg>{1}</errmsg></root>";
            if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, setting.Token))
                return Content(error.Format(400, "参数错误"), Produces.XML);

            postModel.AppId = setting.WeixinAppId;
            postModel.Token = setting.Token;
            postModel.EncodingAESKey = setting.EncodingAESKey;

            //v4.2.2之后的版本，可以设置每个人上下文消息储存的最大数量，防止内存占用过多，如果该参数小于等于0，则不限制
            var maxRecordCount = 0;

            //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
            var messageHandler = new CustomMessageHandler(Request.Body, postModel, maxRecordCount);

            /* 如果需要添加消息去重功能，只需打开OmitRepeatedMessage功能，SDK会自动处理。
             * 收到重复消息通常是因为微信服务器没有及时收到响应，会持续发送2-5条不等的相同内容的RequestMessage*/
            messageHandler.OmitRepeatedMessage = true;

            //测试时可开启此记录，帮助跟踪数据，使用前请确保App_Data文件夹存在，且有读写权限。
            if (setting.IsDebug) messageHandler.SaveRequestMessageLog(MP.RootPath);//记录 Request 日志（可选）

            //执行前，请确保/App_Data/该目录有写入权限。
            await messageHandler.ExecuteAsync(new CancellationToken());//执行微信处理过程（关键）
            if (messageHandler.ResponseDocument == null) return Content(error.Format(400, "消息去重"), Produces.XML);

            if (setting.IsDebug) messageHandler.SaveResponseMessageLog(MP.RootPath);//记录 Response 日志（可选）

            //return Content(messageHandler.ResponseDocument.ToString(), Produces.XML);
            return new FixWeixinBugWeixinResult(messageHandler);
        }

        /// <summary>
        /// 微信授权获取openid
        /// </summary>
        /// <param name="code">客户端获取到的参数,会被传递至微信服务器进行验证.</param>
        /// <param name="state">通过该随机KEY值缓存openid(选填)</param>
        /// <param name="returnUrl">跳转URL(选填)</param>
        [HttpGet("Openid")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Openid(string code, string state, string returnUrl)
        {
            string openid;
            OAuthScope scope = OAuthScope.snsapi_base;
            if (!string.IsNullOrEmpty(state))
            {
                openid = await redis.StringGetAsync(setting.WeixinAppId + scope + state);
                if (openid != null) return Ok(new { openid, state });
            }
            else
            {
                state = Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrEmpty(code))
            {
                returnUrl = OAuthApi.GetAuthorizeUrl(setting.WeixinAppId, Request.RootPath() + "wx/openid", state, scope);
                return Redirect(returnUrl);
            }

            var accessToken = OAuthApi.GetAccessToken(setting.WeixinAppId, setting.WeixinAppSecret, code);
            if (accessToken.errcode != ReturnCode.请求成功)
                return Error(accessToken.errmsg);

            openid = accessToken.openid;
            await redis.StringSetAsync(setting.WeixinAppId + scope + state, openid, redisCacheTime);

            if (string.IsNullOrEmpty(returnUrl))
                return Ok(new { openid, state });

            returnUrl += (returnUrl.Contains("?") ? "&" : "?") + $"state={state}";
            return Redirect(returnUrl);
        }

        /// <summary>
        /// 微信授权获取userinfo
        /// </summary>
        /// <param name="code">客户端获取到的参数,会被传递至微信服务器进行验证.</param>
        /// <param name="state">通过该随机KEY值缓存userinfo(选填)</param>
        /// <param name="openid">通过openid获取userinfo(选填)</param>
        /// <param name="returnUrl">跳转URL(选填)</param>
        [HttpGet("Userinfo")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Userinfo(string code, string state, string openid, string returnUrl)
        {
            bool hasState = false;
            OAuthAccessTokenResult accessToken = null;
            OAuthScope scope = OAuthScope.snsapi_userinfo;
            if (!string.IsNullOrEmpty(state))
            {
                openid = await redis.StringGetAsync(setting.WeixinAppId + scope + state);
                hasState = !string.IsNullOrEmpty(openid);
            }
            else
            {
                state = Guid.NewGuid().ToString("N");
            }
            if (hasState)
            {
                accessToken = await redis.GetAsync<OAuthAccessTokenResult>(setting.WeixinAppId + scope + openid);
                if (accessToken != null && DateTime.TryParse(accessToken.errmsg, out DateTime expiration) && DateTime.Now > expiration)
                {
                    var res = await OAuthApi.RefreshTokenAsync(setting.WeixinAppId, accessToken.refresh_token);
                    if (res.errcode == ReturnCode.请求成功)
                    {
                        accessToken = res;
                        accessToken.errmsg = DateTime.Now.AddSeconds(accessToken.expires_in - 3).ToDateTimeString();
                        await redis.SetAsync(setting.WeixinAppId + scope + openid, accessToken, redisCacheTime);
                    }
                    else
                    {
                        return Error(res.errmsg);
                    }
                }
            }
            if (accessToken == null)
            {
                if (string.IsNullOrEmpty(code))
                {
                    returnUrl = OAuthApi.GetAuthorizeUrl(setting.WeixinAppId, Request.RootPath() + "wx/userinfo", state, scope);
                    return Redirect(returnUrl);
                }

                accessToken = await OAuthApi.GetAccessTokenAsync(setting.WeixinAppId, setting.WeixinAppSecret, code);
                if (accessToken.errcode != ReturnCode.请求成功)
                    return Error(accessToken.errmsg);

                openid = accessToken.openid;
                accessToken.errmsg = DateTime.Now.AddSeconds(accessToken.expires_in - 3).ToDateTimeString();
                await redis.SetAsync(setting.WeixinAppId + scope + openid, accessToken, redisCacheTime);
            }

            openid = accessToken.openid;
            var userinfo = await OAuthApi.GetUserInfoAsync(accessToken.access_token, openid);
            if (string.IsNullOrEmpty(returnUrl))
                return Ok(new { userinfo, state });

            returnUrl += (returnUrl.Contains("?") ? "&" : "?") + $"state={state}";
            return Redirect(returnUrl);
        }

        /// <summary>
        /// 无微信授权获取userinfo(检测用户是否订阅)
        /// </summary>
        /// <param name="state">通过该随机KEY值缓存openid</param>
        /// <param name="openid">通过openid获取userinfo</param>
        [HttpGet("Subscribeinfo")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Subscribeinfo(string state, string openid)
        {
            if (!string.IsNullOrEmpty(state))
            {
                OAuthScope scope = OAuthScope.snsapi_base;
                openid = await redis.StringGetAsync(setting.WeixinAppId + scope + state);
            }
            else
            {
                if (string.IsNullOrEmpty(openid)) return Error("缺少参数");
                state = Guid.NewGuid().ToString("N");
            }

            var userinfo = await redis.GetAsync<UserInfoJson>(setting.WeixinAppId + openid);
            if (userinfo == null)
            {
                var access = WebFramework.Weixins.MP.AccessToken.Get(setting.WeixinAppId);
                if (DateTime.Now > access.Expire) access = WebFramework.Weixins.MP.AccessToken.Get(setting.WeixinAppId, true);
                if (DateTime.Now > access.Expire) throw new Exception("系统异常，请稍候再试！");

                userinfo = await UserApi.InfoAsync(access.Token, openid);
                if (userinfo == null) return Error("无法获取用户的订阅信息");

                await redis.SetAsync(setting.WeixinAppId + openid, userinfo, redisCacheTime);
            }
            return Ok(new { userinfo, state });
        }

    }
}
