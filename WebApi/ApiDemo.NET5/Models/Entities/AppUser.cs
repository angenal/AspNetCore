using System;

namespace ApiDemo.NET5.Models.Entities
{
    public class AppUser : Entity
    {
        public virtual string Username { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual string IdCard { get; set; }
        public virtual string Email { get; set; }
        public virtual string OpenId { get; set; }


        public virtual string Password { get; set; }
        public virtual string PasswordSalt { get; set; }
        public virtual string PasswordResetCode { get; set; }
        public virtual string SecurityStamp { get; set; }


        public virtual int Type { get; set; }
        public virtual int Role { get; set; }


        public virtual string Name { get; set; }
        public virtual string Surname { get; set; }
        public virtual string Nickname { get; set; }
        public virtual string Birthday { get; set; }
        public virtual int Sex { get; set; }
        public virtual string Avatar { get; set; }
        public virtual string NormalizedName { get; set; }
        public virtual string NormalizedEmail { get; set; }
        public virtual string ConcurrencyStamp { get; set; }
        public virtual string Remark { get; set; }


        public virtual bool? IsEmailConfirmed { get; set; }
        public virtual string EmailConfirmationCode { get; set; }
        public virtual bool? IsPhoneNumberConfirmed { get; set; }
        public virtual string PhoneConfirmationCode { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool? IsAdminActive { get; set; }
        public virtual bool? IsTwoFactorEnabled { get; set; }
        public virtual bool? IsLockoutEnabled { get; set; }
        public virtual DateTime? LockoutEndDate { get; set; }
        public virtual DateTime? LastLoginTime { get; set; }
        public virtual string LastLoginIP { get; set; }
        public virtual int AccessFailedCount { get; set; }
        public virtual int AssessType { get; set; }

    }
}
