using LiteDB;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

namespace Identity.LiteDB
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class IdentityRole : IdentityRole<string>
    {
        public IdentityRole() => Id = ObjectId.NewObjectId().ToString();

        public IdentityRole(string roleName) : this() => Name = roleName;
    }
}
