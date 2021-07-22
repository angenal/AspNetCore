using Identity.LiteDB.Models;

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
            IsActive = true;
        }
    }
}
