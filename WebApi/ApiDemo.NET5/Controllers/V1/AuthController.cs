using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using WebFramework;
using WebInterface;

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
        private readonly ILiteDb liteDb;
        private readonly ICrypto crypto;
        private readonly IJwtGenerator JwtToken;

        /// <summary>
        ///
        /// </summary>
        public AuthController(ILiteDb liteDb, ICrypto crypto, IJwtGenerator jwtToken)
        {
            this.liteDb = liteDb;
            this.crypto = crypto;
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
                Name = "测试", // ApiVersionService.Controllers[GetType().FullName].Versions(nameof(TestLogin)).LastOrDefault(),
                Role = "test",
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
            return Ok(new { userid = User.Identity.Name, user });
        }

        /// <summary>
        /// Returns authorized session data for test user.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "test", Roles = "test")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public ActionResult TestRole()
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
