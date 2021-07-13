using System;
using System.Collections.Generic;
using System.Text;

namespace WebCore.Data.DTO
{
    /// <summary>
    /// 基本的用户列表信息
    /// </summary>
    public static class User
    {
        public static List<IUserInfo> Infos;
    }

    /// <summary>
    /// 基本的用户信息
    /// </summary>
    public class UserInfo : IUserInfo
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        public string userid { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 凭证密钥
        /// </summary>
        public string secret { get; set; }
    }
    public interface IUserInfo
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        string userid { get; set; }
        /// <summary>
        /// 凭证密钥
        /// </summary>
        string secret { get; set; }
    }
}
