using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebSwaggerDemo.NET5.Common
{
    /// <summary>
    /// 检查用户操作权限
    /// </summary>
    public class PermissionChecker : IPermissionChecker
    {
        /// <summary>
        /// 所有登录用户的权限集合
        /// </summary>
        private static readonly ConcurrentDictionary<string, string[]> Permissions = new ConcurrentDictionary<string, string[]>();

        /// <summary></summary>
        public async Task<bool> AuthorizeAsync(string permissions, string userIdentityName)
        {
            if (string.IsNullOrEmpty(permissions))
                return true;
            if (string.IsNullOrEmpty(userIdentityName) || !Permissions.ContainsKey(userIdentityName))
                return false;
            return await GetAsync(permissions, Permissions[userIdentityName]);
        }

        /// <summary></summary>
        public async Task SetAsync(string[] permissions, string userIdentityName)
        {
            Permissions.AddOrUpdate(userIdentityName, u => permissions, (u, v) => permissions);
            await Task.CompletedTask;
        }

        /// <summary></summary>
        private static async Task<bool> GetAsync(string permissions, string[] allPermissions)
        {
            bool result = LogicExpression.Eval(permissions, allPermissions);
            return await Task.FromResult(result);
        }
    }
}
