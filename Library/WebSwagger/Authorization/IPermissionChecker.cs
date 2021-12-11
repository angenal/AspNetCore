using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authorization
{
    public interface IPermissionChecker
    {
        /// <summary>
        /// Check whether the current user has some permissions.
        /// </summary>
        /// <param name="permissions">The list of permissions to require for authorization.</param>
        /// <param name="userIdentityName">the name of the current user.</param>
        /// <returns>Gets a value that indicates whether the user has been authenticated.</returns>
        Task<bool> AuthorizeAsync(string permissions, string userIdentityName);
        /// <summary>
        /// Set permissions for the current user.
        /// </summary>
        /// <param name="permissions">The list of permissions to require for authorization.</param>
        /// <param name="userIdentityName">the name of the current user.</param>
        /// <returns></returns>
        Task SetAsync(string[] permissions, string userIdentityName);
    }
}
