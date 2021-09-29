using Microsoft.AspNetCore.Authentication.WxOpen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.WxOpen.AdvancedAPIs;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Sns;
using Senparc.Weixin.WxOpen.Containers;
using Senparc.Weixin.WxOpen.Entities.Request;
using Senparc.Weixin.WxOpen.Helpers;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WebCore;
using WebFramework.Weixins.MessageHandlers.WxOpenMessageHandlers;
using WebFramework.Weixins.MessageTemplate.WxOpen;
using WebInterface;

namespace WebFramework.Controllers
{
    /// <summary>
    /// 微信小程序
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class WxOpenController : ApiController
    {
        private readonly SenparcWeixinSetting setting;
        private readonly ILiteDb liteDb;
        private readonly ICrypto crypto;
        private readonly IJwtGenerator jwtToken;

        /// <summary></summary>
        public WxOpenController(IOptions<SenparcWeixinSetting> setting, ILiteDb liteDb, ICrypto crypto, IJwtGenerator jwtToken)
        {
            this.setting = setting.Value;
            this.liteDb = liteDb;
            this.crypto = crypto;
            this.jwtToken = jwtToken;
        }

        /// <summary>
        /// 微信后台验证地址“接口配置信息”的Url填写如 https://YourDomainName/WxOpen
        /// </summary>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult Get([FromQuery] PostModel postModel, string echostr)
        {
            if (CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, setting.WxOpenToken))
                return Content(echostr); //返回随机字符串则表示验证通过
            return Content("如果你在浏览器中看到这句话，说明此地址可以被作为微信小程序后台的Url，请注意保持Token一致。");
        }

        /// <summary>
        /// 用户发送消息后，微信平台自动Post一个请求到这里，并等待响应XML
        /// </summary>
        [HttpPost]
        [Produces(Produces.XML)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Post([FromQuery] PostModel postModel)
        {
            string error = "<root><errcode>{0}</errcode><errmsg>{1}</errmsg></root>";
            if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, setting.WxOpenToken))
                return Content(error.Format(400, "参数错误"), Produces.XML);

            postModel.AppId = setting.WxOpenAppId;//根据自己后台的设置保持一致
            postModel.Token = setting.WxOpenToken;//根据自己后台的设置保持一致
            postModel.EncodingAESKey = setting.EncodingAESKey;//根据自己后台的设置保持一致

            //v4.2.2之后的版本，可以设置每个人上下文消息储存的最大数量，防止内存占用过多，如果该参数小于等于0，则不限制
            var maxRecordCount = 10;

            //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
            var messageHandler = new CustomWxOpenMessageHandler(HttpContext.Request.Body, postModel, maxRecordCount);

            //如果需要添加消息去重功能，只需打开OmitRepeatedMessage功能，SDK会自动处理。
            //收到重复消息通常是因为微信服务器没有及时收到响应，会持续发送2-5条不等的相同内容的RequestMessage
            //messageHandler.OmitRepeatedMessage = true;

            try
            {
                //测试时可开启此记录，帮助跟踪数据，使用前请确保App_Data文件夹存在，且有读写权限。
                if (Config.IsDebug) messageHandler.SaveRequestMessageLog(Weixins.Data.WxOpen.RootPath);//记录 Request 日志（可选）

                await messageHandler.ExecuteAsync(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);//执行微信处理过程（关键）
                if (messageHandler.ResponseDocument == null) return Content(error.Format(400, "消息去重"), Produces.XML);

                if (Config.IsDebug) messageHandler.SaveResponseMessageLog(Weixins.Data.WxOpen.RootPath);//记录 Response 日志（可选）

                var s = messageHandler.ResponseDocument.ToString();
                return Content(s, Produces.XML);
            }
            catch (Exception ex)
            {
                var s = $" [Post]WxOpen [异常]：{ex.Message}";
                WeixinTrace.Log(s);
                return Content(error.Format(1, ex.Message), Produces.XML);
            }
        }


        /// <summary>
        /// 获取登录凭证 (wx.login登陆成功后发送该请求)
        /// </summary>
        /// <param name="code">客户端获取到的参数,会被传递至微信服务器进行验证.</param>
        [HttpGet("GetToken")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetToken(string code)
        {
            var result = SnsApi.JsCode2Json(setting.WxOpenAppId, setting.WxOpenAppSecret, code);
            if (result.errcode != ReturnCode.请求成功)
                return Error(result.errmsg);

            string openid = result.openid, unionid = result.unionid;

            // 自定义逻辑：处理微信OpenId与该系统的关系
            Session o = await GetSessionByOpenId(openid);
            if (o == null || string.IsNullOrEmpty(o.Id)) return Error("操作失败.");

            //使用SessionContainer管理登录信息（推荐）
            var sessionBag = SessionContainer.UpdateSession(o.Id, result.openid, result.session_key, unionid);
            //注意：生产环境下SessionKey属于敏感信息，不能进行传输！
            string key = sessionBag.Key, sessionKey = sessionBag.SessionKey;
            if (o.Id != key) return Error("操作失败..");

            var session = JObject.FromObject(o);
            session["token"] = jwtToken.Generate(o.Claims());

            return Ok(session);
        }

        /// <summary>
        /// 创建登录凭证 https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/login/auth.code2Session.html
        /// </summary>
        /// <param name="cacheKey">微信服务端返回的会话密匙保存在缓存中关联的Key</param>
        /// <param name="sessionKey">会话密钥(选填)【请注意该信息的安全性】</param>
        /// <param name="openid">用户唯一标识(选填)【请注意该信息的安全性】</param>
        /// <param name="unionid">用户在开放平台的唯一标识符(选填)</param>
        [HttpGet("CreateToken")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateToken(string cacheKey, string sessionKey = "", string openid = "", string unionid = "")
        {
            if (string.IsNullOrWhiteSpace(cacheKey) && string.IsNullOrWhiteSpace(openid)) return Error($"参数{nameof(cacheKey)}不能为空");

            // 获取缓存OpenId后创建登录凭证
            if (string.IsNullOrWhiteSpace(openid))
            {
                var store = HttpContext.RequestServices.GetService<IWxOpenLoginStateInfoStore>();
                if (store == null) return Error("系统服务未配置正确");
                var i = await store.GetSessionInfo(cacheKey);
                if (i == null) return Error($"参数{nameof(cacheKey)}错误");
                openid = i.OpenId;
            }

            // 自定义逻辑：处理微信OpenId与该系统的关系
            Session o = await GetSessionByOpenId(openid);
            if (o == null || string.IsNullOrEmpty(o.Id)) return Error("操作失败.");

            var session = JObject.FromObject(o);
            session["token"] = jwtToken.Generate(o.Claims());

            return Ok(session);
        }

        /// <summary>
        /// 自定义逻辑：处理微信OpenId与该系统的关系
        /// </summary>
        private async Task<Session> GetSessionByOpenId(string openid)
        {
            if (string.IsNullOrEmpty(openid)) throw new Exception($"参数{nameof(openid)}不能为空");
            //TODO 保存微信OpenId
            Session session = null;
            return await Task.FromResult(session);
        }


        /// <summary>
        /// 获取手机号
        /// </summary>
        /// <param name="sessionId">登录凭证id</param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        [HttpGet("GetPhoneNumber")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetPhoneNumber(string sessionId, string encryptedData, string iv)
        {
            try
            {
                var sessionBag = SessionContainer.GetSession(sessionId);
                if (sessionBag == null || sessionBag.OpenId == null)
                    return Error("配置错误或验证失败！");

                var phoneNumber = EncryptHelper.DecryptPhoneNumber(sessionId, encryptedData, iv);
                return Ok(phoneNumber);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 签名校验
        /// </summary>
        /// <param name="sessionId">登录凭证id</param>
        /// <param name="rawData"></param>
        /// <param name="signature"></param>
        [HttpGet("CheckSign")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult CheckSign(string sessionId, string rawData, string signature)
        {
            try
            {
                var ok = EncryptHelper.CheckSignature(sessionId, rawData, signature);
                return Ok(ok ? 1 : 0);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 水印校验
        /// </summary>
        /// <param name="sessionId">登录凭证id</param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        [HttpGet("CheckData")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult CheckData(string sessionId, string encryptedData, string iv)
        {
            try
            {
                var decoded = EncryptHelper.DecodeUserInfoBySessionId(sessionId, encryptedData, iv);
                var ok = decoded != null && decoded.CheckWatermark(setting.WxOpenAppId);
                return Ok(ok ? 1 : 0);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 测试订阅消息接口
        /// </summary>
        /// <param name="sessionId">登录凭证id</param>
        /// <param name="templateId">消息模板id</param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        [HttpGet("TestTemplate")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TestTemplate(string sessionId, string templateId, string title, string content)
        {
            try
            {
                var sessionBag = SessionContainer.GetSession(sessionId);
                var data = new WxOpenTemplateMessage_Notice(templateId, title, content, DateTime.Now);
                var result = await MessageApi.SendSubscribeAsync(setting.WxOpenAppId, sessionBag.OpenId, templateId, data);
                if (result.errcode != ReturnCode.请求成功) return Error(result.errmsg);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
