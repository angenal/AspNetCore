using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebCore.Cache;

namespace WebSwaggerDemo.NET5.Common
{
    /// <summary>
    /// 检查用户权限
    /// </summary>
    public class PermissionChecker : IPermissionChecker
    {
        private readonly PermissionStorage storage = new PermissionStorage();

        /// <summary></summary>
        public async Task<bool> AuthorizeAsync(string permissions, string userIdentityName)
        {
            if (string.IsNullOrEmpty(permissions))
                return true;
            if (string.IsNullOrEmpty(userIdentityName))
                return false;
            string[] allPermissions = storage.Get(userIdentityName);
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
            storage.Set(permissions, userIdentityName);
            await Task.CompletedTask;
        }

        /// <summary></summary>
        public async Task<string[]> GetAsync(string userIdentityName)
        {
            string[] permissions = storage.Get(userIdentityName);
            return await Task.FromResult(permissions);
        }
    }
    public sealed class PermissionStorage
    {
        private static readonly KV<string, PermissionList> kv;
        static PermissionStorage()
        {
            kv = new KV<string, PermissionList>("App_Data");
        }
        public void Set(string[] permissions, string userIdentityName)
        {
            kv.Set(userIdentityName, new PermissionList(permissions));
        }
        public string[] Get(string userIdentityName)
        {
            return kv.Get(userIdentityName)?.ToArray() ?? System.Array.Empty<string>();
        }
    }
    public sealed class PermissionList : List<string>
    {
        public PermissionList()
        {
        }
        public PermissionList(string[] permissions)
        {
            AddRange(permissions);
        }
    }
}
