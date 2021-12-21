using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using WebCore.Cache;

namespace WebSwaggerDemo.NET5.Common
{
    /// <summary>
    /// 检查用户权限
    /// </summary>
    public class PermissionChecker : IPermissionChecker
    {
        private readonly IPermissionStorage storage;

        public PermissionChecker(IPermissionStorage storage)
        {
            this.storage = storage;
        }

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

    public interface IPermissionStorage
    {
        void Set(string[] permissions, string userIdentityName);
        string[] Get(string userIdentityName);
    }

    public sealed class PermissionStorage : IPermissionStorage
    {
        private readonly All kv;

        public PermissionStorage(string redisConnectionstring)
        {
            kv = new All(new Memory(), new Redis(redisConnectionstring));
        }

        public void Set(string[] permissions, string userIdentityName)
        {
            kv.Set(userIdentityName, string.Join(",", permissions));
        }

        public string[] Get(string userIdentityName)
        {
            return kv.Get<string>(userIdentityName)?.Split(',') ?? System.Array.Empty<string>();
        }
    }
}
