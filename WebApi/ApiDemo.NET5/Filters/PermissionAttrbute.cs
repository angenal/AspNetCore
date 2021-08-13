using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;

namespace ApiDemo.NET5.Filters
{
    /// <summary>
    /// 访问授权 Authority Permissions
    /// </summary>
    public class PermissionAttrbute : AuthorizeAttribute
    {
        /// <summary>
        /// 类别
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 权限表的Code唯一编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 权限表[角色:对应权限]
        /// </summary>
        public static readonly Dictionary<string, List<string>> Permissions = new Dictionary<string, List<string>>();

        /// <summary>
        /// 访问授权
        /// </summary>
        /// <param name="code"></param>
        public PermissionAttrbute(string code) => Code = code;

        /// <summary>
        /// 授权验证
        /// </summary>
        /// <param name="roleList"></param>
        /// <returns></returns>
        public bool Valid(IList<string> roleList)
        {
            var code = Code.Trim().Replace(" ", "");
            var codes = code.Split('+', '|').Where(i => i.Length > 0).ToArray();
            if (codes.Length == 0) return true;

            var permissions = new List<string>();
            foreach (string role in roleList)
                if (Permissions.ContainsKey(role)) permissions.AddRange(Permissions[role].ToArray());
            if (permissions.Count == 0) return false;

            var ok = permissions.Contains(codes[0]);
            for (int i = 1, x = codes[0].Length; i < codes.Length; i++)
            {
                ok = code[x] == '+' ? ok && permissions.Contains(codes[i]) : ok || permissions.Contains(codes[i]);
                x += 1 + codes[i].Length;
            }
            return ok;
        }
    }
}
