using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ApiDemo.NET5.Common;
using ApiDemo.NET5.Models.DTO.Auth;
using ApiDemo.NET5.Models.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace ApiDemo.NET5.Controllers.V2
{
    /// <summary>
    /// 认证授权
    /// </summary>
    [ApiController]
    //[ApiExplorerSettings(GroupName = "auth"), Display(Name = "认证系统", Description = "认证系统描述文字")]
    [ApiVersion("2.0")]
    //[Route("api/[controller]/v2/[action]")]
    [Route("api/[controller]/[action]")]
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
        [ApiVersion("2.0"), ApiVersion("2.1")]
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult TestLogin()
        {
            var o = new Session(Guid.NewGuid().ToString(), "User" + new Random().Next(100, 999))
            {
                Name = "测试 Version " + HttpContext.GetRequestedApiVersion()
            };
            var claims = o.Claims();
            var session = JObject.FromObject(o);
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

        /// <summary>
        /// Login with username and password. No previous authorization required.
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult Login([FromBody] LoginInputDto input)
        {
            string username = input.Username.ToLower();

            //string hash = await DB.Value<string>("SELECT hash FROM User WHERE LOWER(username)=@username", new { username });
            //if (!string.IsNullOrEmpty(hash) && Crypto.VerifyHashedPassword(hash, input.Password))
            //    return Ok(LoadSession(true));

            using (var db = DB.Open())
            {
                var c = db.GetCollection<AppUser>(LiteDB.BsonAutoId.Guid);

                var d = c.Query().Where(q => q.Username == username).Select(q => new { q.Id, q.Password, q.PasswordSalt }).FirstOrDefault();

                if (d == null)
                    return BadRequest("Login username error");

                if (!Crypto.VerifyHashedPassword(d.Password, input.Password + d.PasswordSalt))
                    return BadRequest("Login password error");

                var o = c.Query().Where(q => q.Id == d.Id).Select(q => new Session
                {
                    Id = q.Id.ToString(),
                    UserName = q.Username,
                    PhoneNumber = q.PhoneNumber,
                    IdCard = q.IdCard,
                    Email = q.Email,
                    Type = q.Type,
                    Role = q.Role,
                    Name = q.Name,
                    Nickname = q.Nickname,
                    Avatar = q.Avatar,
                }).FirstOrDefault();
                var claims = o.Claims();
                var session = JObject.FromObject(o);
                session["token"] = JwtToken.Generate(claims);
                return Ok(session);
            }
        }

        #endregion

        #region session

        /// <summary>
        /// Returns authorized session data for this user.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public ActionResult Session()
        {
            return Ok(user);
        }

        //private async Task<JObject> LoadSession(string id)
        //{
        //    return await DB.Json("SELECT id, username, email FROM User WHERE id=@id", new { id });
        //}

        #endregion

    }
}
