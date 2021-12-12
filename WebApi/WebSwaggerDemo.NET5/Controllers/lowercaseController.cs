using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSwagger.Attributes;
using WebSwaggerDemo.NET5.Common;
using WebSwaggerDemo.NET5.Models;

namespace WebSwaggerDemo.NET5.Controllers
{
    /// <summary>
    /// 小写控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerApiGroup(GroupSample.Demo)]
    [SwaggerApiGroup(GroupSample.Test)]
    // ReSharper disable once InconsistentNaming
    public class lowercaseController : ControllerBase
    {
        private readonly IJwtGenerator jwtToken;
        private readonly IPermissionChecker permissionChecker;

        public lowercaseController(IJwtGenerator jwtToken, IPermissionChecker permissionChecker)
        {
            this.jwtToken = jwtToken;
            this.permissionChecker = permissionChecker;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="id">唯一标识</param>
        [HttpDelete("{id}")]
        [SwaggerApiGroup(GroupSample.Login), AllowUser("Administrator", "Manager|Delete")]
        [SwaggerResponseHeader(401, "未经授权", "未登录或登录过期")]
        [SwaggerResponseHeader(403, "禁止访问", "无权限或账号异常")]
        public void Delete(int id)
        {
            var user = this.GetSession();
            //根据用户ID获取待删除记录
            string uid = user.Id;
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="permissions">指定需要的权限</param>
        [HttpPost("login")]
        [Produces("application/json")]
        [SwaggerApiGroup(GroupSample.Login), AllowAnonymous]
        [SwaggerResponseHeader(200, "正常", "登录成功后", "{ token }")]
        public async Task<ActionResult> Login(string permissions)
        {
            var o = new Session(Guid.NewGuid().ToString(), "User" + new Random().Next(100, 999))
            {
                Name = "测试",
                Role = "Administrator",
            };
            var claims = o.Claims();
            var session = new JObject();
            var token = jwtToken.Generate(claims);
            session["token"] = token;

            byte[] message = Encoding.ASCII.GetBytes(token), key = Encoding.ASCII.GetBytes(JwtGenerator.Settings.SecretKey.Substring(0, 32));
            var refreshToken = Convert.ToBase64String(Sodium.SecretKeyAuth.Sign(message, key));
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7), // one week expiry time
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            if (!string.IsNullOrWhiteSpace(permissions))
                await permissionChecker.SetAsync(permissions.Split(',', ' '), o.Id);

            return Ok(session);
        }

        /// <summary>
        /// 登录状态
        /// </summary>
        [HttpGet("session")]
        [Produces("application/json")]
        [SwaggerApiGroup(GroupSample.Login), AllowUser]
        [SwaggerResponseHeader(401, "未经授权", "未登录或登录过期")]
        public Session Session()
        {
            return this.GetSession();
        }
    }
}
