using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly ICrypto crypto;
        private readonly IJwtGenerator jwtToken;

        /// <summary></summary>
        public AuthController(ICrypto crypto, IJwtGenerator jwtToken)
        {
            this.crypto = crypto;
            this.jwtToken = jwtToken;
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
            session["token"] = jwtToken.Generate(claims);

            var refreshToken = crypto.RandomString(32);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7), // one week expiry time
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

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
        /// Refresh token test.
        /// </summary>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public IActionResult TestRefreshToken()
        {
            string cookieValue = Request.Cookies["refreshToken"];

            // If cookie is expired then it will give null
            if (cookieValue == null) return Unauthorized();

            return RedirectToAction("TestLogin");
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
