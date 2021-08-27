using Microsoft.AspNetCore.SignalR;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebFramework.Data;

namespace WebFramework.SignalR
{
    /// <summary>
    /// 聊天系统
    /// </summary>
    public class ChatHub : Hub
    {
        /// <summary>
        /// 在线用户列表
        /// </summary>
        public readonly static List<ChatUser> Connections = new List<ChatUser>();
        private readonly static ConcurrentDictionary<string, string> ConnectionsMap = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 数据库访问类 SqlSugar Client
        /// </summary>
        public SqlSugarClient db => _db ??= SQLServerDb.DefaultConnection.NewSqlSugarClient();
        private SqlSugarClient _db;

        /// <summary></summary>
        public ChatHub() { }

        /// <summary>
        /// 加入聊天室
        /// </summary>
        /// <param name="room"></param>
        /// <returns>throw Clients "onError"</returns>
        public async Task Join(string room)
        {
            try
            {
                var user = Context.GetChatUser();

                var item = Connections.Where(u => u.Id == user.Id).FirstOrDefault();
                if (item != null && item.Room != room)
                {
                    // Remove user from others list
                    if (!string.IsNullOrEmpty(user.Room))
                        await Clients.OthersInGroup(user.Room).SendAsync("removeUser", user);

                    // Join to new chat room
                    await Leave(user.Room);
                    await Groups.AddToGroupAsync(Context.ConnectionId, room);
                    user.Room = room;

                    // Tell others to update their list of users
                    await Clients.OthersInGroup(room).SendAsync("addUser", user);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("onError", "Join:" + ex.Message);
            }
        }

        /// <summary>
        /// 获取聊天室的用户列表
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public IEnumerable<ChatUser> GetUsers(string room)
        {
            return Connections.Where(u => u.Room == room).ToList();
        }

        /// <summary>
        /// 发送消息给聊天室的用户
        /// </summary>
        /// <param name="room"></param>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public async Task Send(string room, string userId, string message, string method = "newMessage")
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var clients = new List<IClientProxy>();
                if (!string.IsNullOrEmpty(room)) clients.Add(Clients.Group(room));
                if (!string.IsNullOrEmpty(userId) && ConnectionsMap.TryGetValue(userId, out string connectionId)) clients.Add(Clients.Client(connectionId));

                var id = Context.GetId();
                foreach (var sender in Connections.Where(u => u.Id == id))
                {
                    // Build the message
                    var item = new Message
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", string.Empty),
                        From = sender.Name,
                        Time = DateTime.Now,
                    };

                    // Send the message
                    foreach (IClientProxy client in clients) await client.SendAsync(method, item);
                    //await Clients.Caller.SendAsync("newMessage", item);
                }
            }
        }

        /// <summary>
        /// 离开聊天室
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public async Task Leave(string room)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
        }

        /// <summary>
        /// 连接事件
        /// </summary>
        /// <returns>throw Clients "onError"</returns>
        public override Task OnConnectedAsync()
        {
            Task Abort()
            {
                Context.Abort(); // 关闭连接
                return base.OnConnectedAsync();
            }

            try
            {
                var user = Context.GetChatUser();
                if (string.IsNullOrEmpty(user.Id)) return Abort();

                if (ConnectionsMap.TryAdd(user.Id + user.Device, Context.ConnectionId))
                {
                    // 聊天室(群)
                    var groupName = string.IsNullOrEmpty(user.Room) ? "default" : user.Room;
                    // Adds group
                    Groups.AddToGroupAsync(Context.ConnectionId, groupName).Wait();
                    // Adds user
                    Connections.Add(user);
                    // Remove old group
                    //Groups.RemoveFromGroupAsync(Context.ConnectionId, old);
                }

                //Clients.Caller.SendAsync("getProfileInfo", user.Name, user.Avatar);
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnConnected:" + ex.Message);
            }
            return base.OnConnectedAsync();
        }
        /// <summary>
        /// 断开事件
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>throw Clients "onError"</returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var user = Context.GetIdAndDevice();
                if (user.Id == null) return base.OnDisconnectedAsync(exception);

                // Remove mapping
                ConnectionsMap.TryRemove(user.Id + user.Device, out _);

                // Remove user
                var item = Connections.Where(u => u.Id == user.Id && u.Device == user.Device).First();
                if (item != null)
                {
                    Connections.Remove(item);

                    // Tell other users to remove you from their list
                    //Clients.OthersInGroup(user.Room).SendAsync("removeUser", user);
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnDisconnected: " + ex.Message);
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
