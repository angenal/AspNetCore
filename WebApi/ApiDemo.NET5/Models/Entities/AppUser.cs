using Identity.LiteDB.Models;
using System;

namespace ApiDemo.NET5.Models.Entities
{
    /// <summary>
    /// Application User Account
    /// </summary>
    public class AppUser : IdentityUser
    {
        /// <summary>
        /// New User Account
        /// </summary>
        public AppUser(bool init = true) : base(init)
        {
            if (!init) return;
            Id = Guid.NewGuid().ToString();
            IsActive = true;
        }
    }
}
