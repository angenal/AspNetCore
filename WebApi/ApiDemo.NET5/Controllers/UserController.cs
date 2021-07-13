using ApiDemo.NET5.Models.DTO.User;
using ApiDemo.NET5.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
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
        public UserController(ILiteDb db, ICrypto crypto)
        {
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

            using (var db = DB.Open())
            {
                var c = db.GetCollection<AppUser>(LiteDB.BsonAutoId.Guid);

                if (c.Exists(q => q.Username == input.Username))
                    return BadRequest("Username is duplicated");

                if (!string.IsNullOrWhiteSpace(input.Email) && c.Exists(q => q.Email == input.Email))
                    return BadRequest("Email is duplicated");

                if (!string.IsNullOrWhiteSpace(input.PhoneNumber) && c.Exists(q => q.PhoneNumber == input.PhoneNumber))
                    return BadRequest("PhoneNumber is duplicated");

                if (!string.IsNullOrWhiteSpace(input.IdCard) && c.Exists(q => q.IdCard == input.IdCard))
                    return BadRequest("IdCard is duplicated");

                var salt = new Random().Next(1000, 9999).ToString();
                var hash = Crypto.HashPassword(input.Password + salt);
                var entity = new AppUser
                {
                    CreationTime = DateTime.Now,
                    IsActive = true,
                    Username = input.Username,
                    PhoneNumber = input.PhoneNumber,
                    IdCard = input.IdCard,
                    Email = input.Email,
                    Password = hash,
                    PasswordSalt = salt,
                };
                entity.LastModificationTime = entity.CreationTime;
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

    }
}
