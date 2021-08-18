using ApiDemo.NET5.Models.DTO.User;
using ApiDemo.NET5.Models.Entities;
using Microsoft.AspNetCore.Authentication.QQ;
using Microsoft.AspNetCore.Authentication.Weixin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net;
using System.Threading.Tasks;
using WebFramework;
using WebInterface;

namespace ApiDemo.NET5.Controllers
{
    /// <summary>
    /// 用户账号
    /// </summary>
    [ApiController]
    [Authorize]
    //[ApiExplorerSettings(GroupName = "demo"), Display(Name = "演示系统", Description = "演示系统描述文字")]
    [ApiVersion("1.0")]
    [Route("api/[controller]/[action]")]
    //[Route("{culture:culture}/[controller]/[action]")]
    public class UserController : ApiController
    {
        private readonly ILiteDb liteDb;
        private readonly ICrypto crypto;
        private readonly IMemoryCache cache;

        /// <summary>
        ///
        /// </summary>
        public UserController(ILiteDb liteDb, ICrypto crypto, IMemoryCache cache)
        {
            this.liteDb = liteDb;
            this.crypto = crypto;
            this.cache = cache;
        }

        #region api/user/register

        /// <summary>
        /// Register with username and password. No previous authorization required.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(RegisterOutputDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult Register([FromBody] RegisterInputDto input)
        {
            //string username = input.Username.ToLower();
            //string email = input.Email?.ToLower();
            //string phone = input.PhoneNumber;
            //string idcard = input.IdCard;

            //if (await DB.Value<int>("SELECT COUNT(*) FROM User WHERE LOWER(username)=@username", new { username }) > 0)
            //    return BadRequest("Duplicated username is not allowed.");

            //var parameters = new { username, hash = Crypto.HashPassword(input.Password), email, phone, idcard };
            //await DB.Execute("INSERT INTO User (username, hash, email, phone, idcard) VALUES (@username, @hash, @email, @phone, @idcard)", parameters);

            using (var db = liteDb.Open())
            {
                var c = db.GetCollection<AppUser>();

                if (c.Exists(q => q.UserName == input.Username))
                    return BadRequest("Username is duplicated");

                if (!string.IsNullOrWhiteSpace(input.Email) && c.Exists(q => q.Email == input.Email))
                    return BadRequest("Email is duplicated");

                if (!string.IsNullOrWhiteSpace(input.PhoneNumber) && c.Exists(q => q.PhoneNumber == input.PhoneNumber))
                    return BadRequest("PhoneNumber is duplicated");

                if (!string.IsNullOrWhiteSpace(input.IdCard) && c.Exists(q => q.IdCard == input.IdCard))
                    return BadRequest("IdCard is duplicated");

                var salt = new Random().Next(1000, 9999).ToString();
                var hash = crypto.HashPassword(input.Password + salt);
                var entity = new AppUser()
                {
                    UserName = input.Username,
                    PhoneNumber = input.PhoneNumber,
                    IdCard = input.IdCard,
                    Email = input.Email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                };
                var id = c.Insert(entity);
                return Ok(new RegisterOutputDto { Id = id.AsGuid });
            }
        }

        #endregion


        /// <summary>
        /// Returns current user info.
        /// </summary>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public ActionResult Info()
        {
            return Ok(user);
        }


        #region get external login information when ( QQ, Weixin ) login success

        // api/User/QQLogin

        /// <summary>
        /// QQ登陆
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <param name="remoteError"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> QQLogin(string returnUrl = null, string remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError)) return BadRequest(remoteError);

            // get information from HttpContext using Microsoft.AspNetCore.Authentication.QQ
            var loginInfo = await HttpContext.GetExternalQQLoginInfoAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                //cache.Set();
            }

            return Ok(loginInfo);
        }

        // api/User/WxLogin

        /// <summary>
        /// 微信登陆
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <param name="remoteError"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> WxLogin(string returnUrl = null, string remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError)) return BadRequest(remoteError);

            // get information from HttpContext using Microsoft.AspNetCore.Authentication.Weixin
            var loginInfo = await HttpContext.GetExternalWeixinLoginInfoAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                //cache.Set();
            }

            return Ok(loginInfo);
        }

        #endregion

    }
}
