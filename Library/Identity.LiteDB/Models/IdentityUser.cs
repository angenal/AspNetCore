using Microsoft.AspNetCore.Identity;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Identity.LiteDB.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    /// <summary>
    /// User Account to Represents a user in the identity system.
    /// The Id property is initialized to form a new GUID string value.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class IdentityUser : IdentityUser<string>
    {
        /// <summary>
        /// Initializes a new instance of IdentityUser.
        /// </summary>
        public IdentityUser()
        {
            Initialize();
        }
        /// <summary>
        /// Initializes a new instance of IdentityUser.
        /// </summary>
        /// <param name="userName">The user name.</param>
        public IdentityUser(string userName)
        {
            UserName = userName;
            Initialize();
        }
        /// <summary>
        /// Initializes a new instance of IdentityUser.
        /// </summary>
        /// <param name="init">Want to initialize?</param>
        public IdentityUser(bool init)
        {
            if (init) Initialize();
        }
        void Initialize()
        {
            Id = Guid.NewGuid().ToString();
            CreationTime = DateTime.Now;
            LastModificationTime = CreationTime;
        }

        public virtual string IdCard { get; set; }
        public virtual string OpenId { get; set; }
        public virtual string PasswordSalt { get; set; }

        public virtual int Type { get; set; }
        public virtual int Role { get; set; }
        public virtual string Name { get; set; }
        public virtual int Sex { get; set; }
        public virtual string Nickname { get; set; }
        public virtual string Birthday { get; set; }
        public virtual string Avatar { get; set; }

        public virtual string Remark { get; set; }

        public virtual string LastLoginIP { get; set; }

        public string AuthenticationKey { get; set; }

        public virtual bool IsActive { get; set; }

        public DateTime? EmailConfirmedTime { get; internal set; }

        public DateTime? PhoneNumberConfirmedTime { get; internal set; }

        public DateTimeOffset? LockoutEndDate { get; internal set; }

        public DateTime CreationTime { get; internal set; }

        public DateTime LastModificationTime { get; set; }
    }
}
