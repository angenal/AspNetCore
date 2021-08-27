using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using WebInterface.Settings;

namespace WebFramework.SignalR
{
    // --------------------------
    // ASP.NET Core SignalR JavaScript 客户端 https://docs.microsoft.com/zh-cn/aspnet/core/signalr/javascript-client?view=aspnetcore-5.0
    // 配置文档参考 https://docs.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-5.0&tabs=javascript#configure-additional-options
    // --------------------------
    /*
    var q = { "sid": "b970d79f-6c9c-41a9-8e54-3bb0a5fb9274", "name": "user login name", "role": "users", "device": "web", "room": "1" };
    var qs = "?sid=" + q.sid + "&name=" + encodeURIComponent(q.name) + "&role=" + encodeURIComponent(q.role) + "&device=" + q.device + "&room=" + q.room;

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/chat" + qs, {
            //accessTokenFactory: function () { return '$token' }, // Get and return the access token.
            //headers: q, // Get and return the user info, only for LongPolling.
            transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
        })
        //.withAutomaticReconnect([0, 3000, 5000, 10000, 15000, 30000]) // If no parameters can be set, it is the default setting.
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // start connect...
    connection.start().then(function () {
        console.log('SignalR Started...')
    }).catch(function (err) {
        return console.error(err);
    });

    // server debugger and tracer
    connection.on("onDebug", function (message) {
        console.log(message);
    });
    // server throw errors
    connection.on("onError", function (message) {
        $("#errorAlert").html(message).removeClass("d-none").show().delay(5000).fadeOut(500);
    });
    // receive messages...
    connection.on("newMessage", function (messages) {
        // logic is here.
    });
     */

    /// <summary>
    /// 聊天用户
    /// </summary>
    public class ChatUser : User
    {
        /// <summary>
        /// 聊天室(群)
        /// from Query["room"]
        /// </summary>
        public string Room { get; set; }
    }

    /// <summary>
    /// 用户信息
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户Id
        /// from JwtRegisteredClaimNames.Sid
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 用户名
        /// from JwtRegisteredClaimNames.Sub
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 角色(类别)
        /// from JwtSettings.RoleClaimType
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 设备标识
        /// from Query["device"]
        /// </summary>
        public string Device { get; set; }
    }

    /// <summary>
    /// Gets the chat user from the hub caller connection.
    /// </summary>
    public static class HubCallerContextChatUserExtensions
    {
        /// <summary>
        /// Gets the chat user from the hub caller connection.
        /// </summary>
        /// <param name="Context">A context abstraction for accessing information.</param>
        /// <returns></returns>
        public static ChatUser GetChatUser(this HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            var sid = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sid);

            var user = !string.IsNullOrEmpty(sid) ? new ChatUser
            {
                Id = sid,
                Name = Context.User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                Role = string.Join(',', Context.User.FindAll(t => t.Type.Equals(JwtSettings.RoleClaimType)).Select(t => t.Value)),
            } : new ChatUser
            {
                Id = req.Query.ContainsKey("sid") ? req.Query["sid"].ToString() : req.Headers.ContainsKey("sid") ? req.Headers["sid"].ToString() : Context.User?.Identity?.Name,
                Name = req.Query.ContainsKey("name") ? req.Query["name"].ToString() : req.Headers.ContainsKey("name") ? req.Headers["name"].ToString() : null,
                Role = req.Query.ContainsKey("role") ? req.Query["role"].ToString() : req.Headers.ContainsKey("role") ? req.Headers["role"].ToString() : null,
            };

            user.Room = req.Query.ContainsKey("room") ? req.Query["room"].ToString() : req.Headers.ContainsKey("room") ? req.Headers["room"].ToString() : Context.User?.FindFirstValue("room");

            var device = req.Query.ContainsKey("device") ? req.Query["device"].ToString() : req.Headers.ContainsKey("device") ? req.Headers["device"].ToString() : "web";
            user.Device = !string.IsNullOrEmpty(device) ? device : "web";

            return user;
        }

        /// <summary>
        /// Gets the user from the hub caller connection.
        /// </summary>
        /// <param name="Context">A context abstraction for accessing information.</param>
        /// <returns></returns>
        public static User GetUser(this HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            var sid = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sid);

            var user = !string.IsNullOrEmpty(sid) ? new User
            {
                Id = sid,
                Name = Context.User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                Role = string.Join(',', Context.User.FindAll(t => t.Type.Equals(JwtSettings.RoleClaimType)).Select(t => t.Value)),
            } : new User
            {
                Id = req.Query.ContainsKey("sid") ? req.Query["sid"].ToString() : req.Headers.ContainsKey("sid") ? req.Headers["sid"].ToString() : Context.User?.Identity?.Name,
                Name = req.Query.ContainsKey("name") ? req.Query["name"].ToString() : req.Headers.ContainsKey("name") ? req.Headers["name"].ToString() : null,
                Role = req.Query.ContainsKey("role") ? req.Query["role"].ToString() : req.Headers.ContainsKey("role") ? req.Headers["role"].ToString() : null,
            };

            var device = req.Query.ContainsKey("device") ? req.Query["device"].ToString() : req.Headers.ContainsKey("device") ? req.Headers["device"].ToString() : "web";
            user.Device = !string.IsNullOrEmpty(device) ? device : "web";

            return user;
        }

        /// <summary>
        /// Gets user id or User.Identity.Name.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static string GetId(this HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            var id = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            return !string.IsNullOrEmpty(id) ? id : req.Query.ContainsKey("sid") ? req.Query["sid"].ToString() : req.Headers.ContainsKey("sid") ? req.Headers["sid"].ToString() : Context.User?.Identity?.Name;
        }

        /// <summary>
        /// Gets user name.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static string GetName(this HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            var name = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return !string.IsNullOrEmpty(name) ? name : req.Query.ContainsKey("name") ? req.Query["name"].ToString() : req.Headers.ContainsKey("name") ? req.Headers["name"].ToString() : null;
        }

        /// <summary>
        /// Gets user role.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static string GetRole(this HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            var roles = Context.User?.FindAll(t => t.Type.Equals(JwtSettings.RoleClaimType))?.Select(t => t.Value);
            return roles != null && roles.Any() ? string.Join(',', roles) : req.Query.ContainsKey("role") ? req.Query["role"].ToString() : req.Headers.ContainsKey("role") ? req.Headers["role"].ToString() : null;
        }

        /// <summary>
        /// Gets user device.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static string GetDevice(this HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            var device = req.Query.ContainsKey("device") ? req.Query["device"].ToString() : req.Headers.ContainsKey("device") ? req.Headers["device"].ToString() : "web";
            return !string.IsNullOrEmpty(device) ? device : "web";
        }

        /// <summary>
        /// Gets the chat room.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static string GetRoom(this HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            return req.Query.ContainsKey("room") ? req.Query["room"].ToString() : req.Headers.ContainsKey("room") ? req.Headers["room"].ToString() : Context.User?.FindFirstValue("room");
        }

        /// <summary>
        /// Gets user id and device.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static User GetIdAndDevice(this HubCallerContext Context)
        {
            var req = Context.GetHttpContext().Request;
            var id = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            id = !string.IsNullOrEmpty(id) ? id : req.Query.ContainsKey("sid") ? req.Query["sid"].ToString() : req.Headers.ContainsKey("sid") ? req.Headers["sid"].ToString() : Context.User?.Identity?.Name;
            var device = req.Query.ContainsKey("device") ? req.Query["device"].ToString() : req.Headers.ContainsKey("device") ? req.Headers["device"].ToString() : "web";
            return new User { Id = id, Device = !string.IsNullOrEmpty(device) ? device : "web" };
        }
    }
}
