using ApiDemo.NET5.Models.DTO.User;
using ApiDemo.NET5.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using WebCore;

namespace ApiDemo.NET5.Controllers
{
    public partial class UserController
    {

        /// <summary>
        /// 注册
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Consumes(Produces.JSON)]
        [Produces(Produces.JSON)]
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

    }
}
