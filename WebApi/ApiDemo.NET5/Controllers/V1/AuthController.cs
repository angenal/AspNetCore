using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ApiDemo.NET5.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace ApiDemo.NET5.Controllers.V1
{
    /// <summary>
    /// 认证授权
    /// </summary>
    [ApiController]
    //[ApiExplorerSettings(GroupName = "auth"), Display(Name = "认证系统", Description = "认证系统描述文字")]
    [ApiVersion("1.0")]
    [ApiVersion("1.2")]
    //[Route("api/[controller]/v1/[action]")]
    [Route("api/[controller]/v{version:version}/[action]")]
    public class AuthController : ApiController
    {
        private readonly IJwtGenerator JwtToken;

        public AuthController(ILiteDb db, ICrypto crypto, IJwtGenerator jwtToken) : base(db, crypto)
        {
            JwtToken = jwtToken;
        }


        #region test login

        /// <summary>
        /// Login test.
        /// </summary>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult TestLogin()
        {
            var o = new Session(Guid.NewGuid().ToString(), "User" + new Random().Next(100, 999))
            {
                Name = "测试 Version " + HttpContext.GetRequestedApiVersion() // ApiVersionService.Controllers[GetType().FullName].Versions(nameof(TestLogin)).LastOrDefault(),
            };
            var claims = o.Claims();
            var session = new JObject();
            session["token"] = JwtToken.Generate(claims);
            return Ok(session);
        }

        /// <summary>
        /// Returns authorized session data for test user.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public ActionResult TestSession()
        {
            return Ok(user);
        }

        #endregion


        #region api/auth/login


        #endregion

        #region session


        #endregion

    }
}
