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
        /// https://docs.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-5.0&tabs=javascript#configure-additional-options
        /// </summary>
        /// <param name="Context">A context abstraction for accessing information.</param>
        /// <returns></returns>
        public static ChatUser Get(HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            var id = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            var jwt = !string.IsNullOrEmpty(id);

            /*
            let connection = new signalR.HubConnectionBuilder()
                .withUrl("/chat", {
                    // accessTokenFactory: () => { return '$token' }, // Get and return the access token.
                    headers: { "id": '$id', "name": '$name', "room": '$room' }, // Get and return the user info
                    transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
                })
                .configureLogging(signalR.LogLevel.Information)
                .build();
             */

            var user = jwt ? new ChatUser
            {
                Id = id,
                Name = Context.User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                Role = Context.User.FindFirstValue("role"),
            } : new ChatUser
            {
                Id = req.Headers.ContainsKey("id") ? req.Headers["id"].ToString() : req.Query.ContainsKey("id") ? req.Query["id"].ToString() : null,
                Name = req.Headers.ContainsKey("name") ? req.Headers["name"].ToString() : req.Query.ContainsKey("name") ? req.Query["name"].ToString() : Context.User?.Identity?.Name,
                Role = req.Headers.ContainsKey("role") ? req.Headers["role"].ToString() : req.Query.ContainsKey("role") ? req.Query["role"].ToString() : null,
            };

            user.Room = req.Headers.ContainsKey("room") ? req.Headers["room"].ToString() : req.Query.ContainsKey("room") ? req.Query["room"].ToString() : Context.User?.FindFirstValue("room");

            var device = req.Headers.ContainsKey("device") ? req.Headers["device"].ToString() : req.Query.TryGetValue("device", out var query) ? query.ToString() : "web";
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
            var req = Context.GetHttpContext().Request;
            var id = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            return !string.IsNullOrEmpty(id) ? id : req.Headers.ContainsKey("id") ? req.Headers["id"].ToString() : req.Query.ContainsKey("id") ? req.Query["id"].ToString() : null;
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
            return !string.IsNullOrEmpty(name) ? name : req.Headers.ContainsKey("name") ? req.Headers["name"].ToString() : req.Query.ContainsKey("name") ? req.Query["name"].ToString() : Context.User?.Identity?.Name;
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
