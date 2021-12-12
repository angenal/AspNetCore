using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebSwaggerDemo.NET5.Common
{
    /// <summary>
    /// 检查用户权限
    /// </summary>
    public class PermissionChecker : IPermissionChecker
    {
        /// <summary></summary>
        private static readonly ConcurrentDictionary<string, string[]> Permissions = new ConcurrentDictionary<string, string[]>();

        /// <summary></summary>
        public async Task<bool> AuthorizeAsync(string permissions, string userIdentityName)
        {
            if (string.IsNullOrEmpty(permissions))
                return true;
            if (string.IsNullOrEmpty(userIdentityName) || !Permissions.ContainsKey(userIdentityName))
                return false;
            string[] allPermissions = Permissions[userIdentityName];
            return await AuthorizeAsync(permissions, allPermissions);
        }

        /// <summary></summary>
        private static async Task<bool> AuthorizeAsync(string permissions, string[] allPermissions)
        {
            bool result = LogicExpression.Eval(permissions, allPermissions);
            return await Task.FromResult(result);
        }

        /// <summary></summary>
        public async Task SetAsync(string[] permissions, string userIdentityName)
        {
            Permissions[userIdentityName] = permissions;
            await Task.CompletedTask;
        }

        /// <summary></summary>
        public async Task<string[]> GetAsync(string userIdentityName)
        {
            string[] permissions = Permissions.ContainsKey(userIdentityName) ? Permissions[userIdentityName] : System.Array.Empty<string>();
            return await Task.FromResult(permissions);
        }
    }
}
