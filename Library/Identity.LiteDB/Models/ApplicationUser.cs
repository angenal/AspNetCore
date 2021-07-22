using LiteDB;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace Identity.LiteDB.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    /// <summary>
    /// User Account to Represents a user in the identity system.
    /// The Id property is initialized to form a new GUID string value.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() : base(false)
        {
            Id = ObjectId.NewObjectId().ToString();
            CreationTime = DateTime.Now;
            LastModificationTime = CreationTime;
            IsActive = true;
            Roles = new List<string>();
            Logins = new List<UserLoginInfo>();
            SerializableLogins = new List<SerializableUserLoginInfo>();
            Claims = new List<IdentityUserClaim<string>>();
            Tokens = new List<UserToken<string>>();
        }

        public List<string> Roles { get; set; }

        public List<UserToken<string>> Tokens { get; set; }

        public List<IdentityUserClaim<string>> Claims { get; set; }

        public List<SerializableUserLoginInfo> SerializableLogins { get; set; }

        [BsonIgnore]
        public List<UserLoginInfo> Logins
        {
            get => SerializableLogins?.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, "")).ToList() ?? new List<UserLoginInfo>();
            set => SerializableLogins = value?.Select(x => new SerializableUserLoginInfo(x.LoginProvider, x.ProviderKey)).ToList() ?? new List<SerializableUserLoginInfo>();
        }

        public virtual void AddRole(string role) => Roles.Add(role);

        public virtual void RemoveRole(string role) => Roles.Remove(role);

        public virtual void AddLogin(UserLoginInfo login) => SerializableLogins.Add(new SerializableUserLoginInfo(login.LoginProvider, login.ProviderKey));

        public virtual void RemoveLogin(UserLoginInfo login) => SerializableLogins = SerializableLogins.Except(SerializableLogins.Where(l => l.LoginProvider == login.LoginProvider).Where(l => l.ProviderKey == login.ProviderKey)).ToList();

        public virtual bool HasPassword() => false;

        public virtual void RemoveClaim(Claim claim) => Claims = Claims.Except(Claims.Where(c => c.ClaimType == claim.Type).Where(c => c.ClaimValue == claim.Value)).ToList();

        public virtual void AddClaim(Claim claim) => Claims.Add(new IdentityUserClaim<string>() { ClaimType = claim.Type, ClaimValue = claim.Value, UserId = Id });

        public void RemoveToken(string loginProvider, string name) => Tokens = Tokens.Except(Tokens.Where(t => t.LoginProvider == loginProvider && t.TokenName == name)).ToList();

        public void AddToken(UserToken<string> token)
        {
            var existingToken = Tokens.SingleOrDefault(t => t.LoginProvider == token.LoginProvider && t.TokenName == token.TokenName);
            if (existingToken == null) Tokens.Add(token);
            else existingToken.TokenValue = token.TokenValue;
        }
    }
}
