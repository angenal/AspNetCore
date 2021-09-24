using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;
using WebFramework.Authentication.WeChat.WxOpen;

namespace WebFramework.Controllers
{
    /// <summary>
    /// 微信小程序
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class WxOpenController : ApiController
    {
        private readonly IWxOpenLoginStateInfoStore store;
        private readonly IJwtGenerator jwtToken;

        /// <summary></summary>
        public WxOpenController(IWxOpenLoginStateInfoStore store, IJwtGenerator jwtToken)
        {
            this.store = store;
            this.jwtToken = jwtToken;
        }

        /// <summary>
        /// 创建登录凭证 https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/login/auth.code2Session.html
        /// </summary>
        /// <param name="cacheKey">微信服务端返回的会话密匙保存在缓存中关联的Key</param>
        /// <param name="sessionKey">会话密钥(选填)【请注意该信息的安全性】</param>
        /// <param name="openid">用户唯一标识(选填)【请注意该信息的安全性】</param>
        /// <param name="unionid">用户在开放平台的唯一标识符(选填)【请注意该信息的安全性】</param>
        [HttpGet("CreateToken")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateToken(string cacheKey, string sessionKey = "", string openid = "", string unionid = "")
        {
            if (string.IsNullOrWhiteSpace(cacheKey) && string.IsNullOrWhiteSpace(openid)) return Error($"参数{nameof(cacheKey)}不能为空");

            // 获取缓存OpenId后创建登录凭证
            if (string.IsNullOrWhiteSpace(openid))
            {
                var i = await store.GetSessionInfo(cacheKey);
                if (i == null) return Error($"参数{nameof(cacheKey)}错误");
                openid = i.OpenId;
            }

            // 自定义逻辑：处理微信OpenId与该系统的关系
            Session o = await GetSessionByOpenId(openid);
            if (o == null || string.IsNullOrEmpty(o.Id)) return Error($"保存微信{nameof(openid)}失败");

            var session = JObject.FromObject(o);
            if (!string.IsNullOrEmpty(o.Id)) session["token"] = jwtToken.Generate(o.Claims());

            return Ok(session);
        }

        /// <summary>
        /// 自定义逻辑：处理微信OpenId与该系统的关系
        /// </summary>
        private async Task<Session> GetSessionByOpenId(string openid)
        {
            if (string.IsNullOrEmpty(openid)) throw new Exception($"参数{nameof(openid)}不能为空");
            // 保存微信OpenId
            Session session = null;
            return await Task.FromResult(session);
        }

        ///// <summary>
        ///// 登录凭证校验
        ///// </summary>
        ///// <param name="code">被传递至微信服务器进行验证,换取用户信息</param>
        //[HttpGet]
        //[Produces("application/json")]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //public IActionResult Signin(string code)
        //{
        //    return Ok(code);
        //}

    }
}
