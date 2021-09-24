using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using WebFramework.Authentication.WeChat.WxOpen;

namespace WebFramework.Controllers
{
    /// <summary>
    /// 微信小程序
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class WxOpenController : ApiController
    {
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration config;
        private readonly IWxOpenLoginStateInfoStore store;
        private readonly IJwtGenerator jwtToken;

        /// <summary></summary>
        public WxOpenController(IWebHostEnvironment env, IConfiguration config, IWxOpenLoginStateInfoStore store, IJwtGenerator jwtToken)
        {
            this.env = env;
            this.config = config;
            this.store = store;
            this.jwtToken = jwtToken;
        }

        /// <summary>
        /// 创建登录凭证
        /// </summary>
        /// <param name="key">微信服务端返回的密匙保存在缓存中关联的Key</param>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateToken(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Error($"参数 key 不能为空");

            // 获取缓存OpenId后创建登录凭证
            var i = await store.GetSessionInfo(key);
            if (i == null || string.IsNullOrEmpty(i.OpenId))
                return Error($"参数 key 错误");

            // 自定义逻辑：处理微信OpenId与该系统的关系
            Session o = await GetSessionByOpenId(i.OpenId);
            if (o == null || string.IsNullOrEmpty(o.Id))
                return Error($"参数 key 错误");

            var session = JObject.FromObject(o);
            if (!string.IsNullOrEmpty(o.Id)) session["token"] = jwtToken.Generate(o.Claims());

            return Ok(session);
        }

        /// <summary>
        /// 自定义逻辑：处理微信OpenId与该系统的关系
        /// </summary>
        private async Task<Session> GetSessionByOpenId(string openId)
        {
            Session session = null;
            return await Task.FromResult(session);
        }

        /// <summary>
        /// 登录凭证校验
        /// </summary>
        /// <param name="code">被传递至微信服务器进行验证,换取用户信息</param>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult Signin(string code)
        {
            return Ok(code);
        }
    }
}
