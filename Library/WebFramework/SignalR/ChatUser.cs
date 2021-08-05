using Microsoft.AspNetCore.SignalR;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebFramework.SignalR
{
    /// <summary>
    /// 聊天用户
    /// </summary>
    public class ChatUser : User
    {
        /// <summary>
        /// Gets the user from the hub caller connection.
        /// </summary>
        /// <param name="Context">A context abstraction for accessing information.</param>
        /// <returns></returns>
        public static ChatUser Get(HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            var id = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            var jwt = !string.IsNullOrEmpty(id);

            var user = jwt ? new ChatUser
            {
                Id = id,
                Name = Context.User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                Role = Context.User.FindFirstValue("role"),
            } : new ChatUser
            {
                Id = req.Query["id"].ToString(),
                Name = req.Query.ContainsKey("name") ? req.Query["name"].ToString() : Context.User?.Identity?.Name,
                Role = req.Query["role"].ToString(),
            };

            user.Room = req.Query.ContainsKey("room") ? req.Query["room"].ToString() : Context.User?.FindFirstValue("room");

            var device = req.Query.TryGetValue("device", out var query) ? query.ToString() : req.Headers["device"].ToString();
            user.Device = !string.IsNullOrEmpty(device) && (device.Equals("desktop", StringComparison.OrdinalIgnoreCase) || device.Equals("mobile", StringComparison.OrdinalIgnoreCase)) ? device : "web";

            return user;
        }

        /// <summary>
        /// Gets User Id.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static string GetId(HubCallerContext Context)
        {
            var id = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            return !string.IsNullOrEmpty(id) ? id : Context.GetHttpContext().Request.Query["id"].ToString();
        }

        /// <summary>
        /// Gets User Name.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static string GetName(HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            var name = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return !string.IsNullOrEmpty(name) ? name : req.Query.ContainsKey("name") ? req.Query["name"].ToString() : Context.User?.Identity?.Name;
        }

        /// <summary>
        /// 聊天室(群)
        /// </summary>
        public string Room { get; set; }
        /// <summary>
        /// 设备标识
        /// </summary>
        public string Device { get; set; }
    }

    /// <summary>
    /// 用户信息
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 角色(类别)
        /// </summary>
        public string Role { get; set; }
    }
}
