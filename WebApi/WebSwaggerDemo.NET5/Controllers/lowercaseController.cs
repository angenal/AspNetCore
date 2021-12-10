using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
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

        public lowercaseController(IJwtGenerator jwtToken)
        {
            this.jwtToken = jwtToken;
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
        [SwaggerApiGroup(GroupSample.Login), Operation("Administrator", "Manager|Delete")]
        [SwaggerResponseHeader(403, "异常", "", "未授权访问")]
        public void Delete(int id)
        {
            var user = this.GetSession();
            string uid = user.Id;
            //根据用户ID获取权限
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [Produces("application/json")]
        [SwaggerApiGroup(GroupSample.Login), SwaggerResponseHeader(200, "正常", "{ token }", "登录成功后")]
        public ActionResult Login()
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
            return Ok(session);
        }

        /// <summary>
        /// 登录状态
        /// </summary>
        [HttpGet("session")]
        [Authorize]
        [Produces("application/json")]
        [SwaggerApiGroup(GroupSample.Login), SwaggerResponseHeader(new int[] { 200, 401 }, "正常", "{ user }", "登录成功后")]
        public ActionResult Session()
        {
            var user = this.GetSession();
            return Ok(new { user });
        }
    }
}
