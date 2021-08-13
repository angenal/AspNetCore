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
            /*
            var q = { "sid": "b970d79f-6c9c-41a9-8e54-3bb0a5fb9274", "name": "your name", "room": "1" };
            var qs = "?sid=" + q.sid + "&name=" + encodeURIComponent(q.name) + "&room=" + q.room;

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

            var req = Context.GetHttpContext().Request;
            var sid = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sid);

            var user = !string.IsNullOrEmpty(sid) ? new ChatUser
            {
                Id = sid,
                Name = Context.User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                Role = Context.User.FindFirstValue("role"),
            } : new ChatUser
            {
                Id = req.Query.ContainsKey("sid") ? req.Query["sid"].ToString() : req.Headers.ContainsKey("sid") ? req.Headers["sid"].ToString() : null,
                Name = req.Query.ContainsKey("name") ? req.Query["name"].ToString() : req.Headers.ContainsKey("name") ? req.Headers["name"].ToString() : Context.User?.Identity?.Name,
                Role = req.Query.ContainsKey("role") ? req.Query["role"].ToString() : req.Headers.ContainsKey("role") ? req.Headers["role"].ToString() : null,
            };

            user.Room = req.Query.ContainsKey("room") ? req.Query["room"].ToString() : req.Headers.ContainsKey("room") ? req.Headers["room"].ToString() : Context.User?.FindFirstValue("room");

            var device = req.Query.ContainsKey("device") ? req.Query["device"].ToString() : req.Headers.ContainsKey("device") ? req.Headers["device"].ToString() : "web";
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
            return !string.IsNullOrEmpty(id) ? id : req.Query.ContainsKey("sid") ? req.Query["sid"].ToString() : req.Headers.ContainsKey("sid") ? req.Headers["sid"].ToString() : null;
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
            return !string.IsNullOrEmpty(name) ? name : req.Query.ContainsKey("name") ? req.Query["name"].ToString() : req.Headers.ContainsKey("name") ? req.Headers["name"].ToString() : Context.User?.Identity?.Name;
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
