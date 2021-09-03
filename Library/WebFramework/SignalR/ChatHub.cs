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
        /// 在线用户列表.
        /// </summary>
        private readonly static List<ChatUser> Connections = new List<ChatUser>();
        /// <summary>
        /// 在线用户连接ID.
        /// </summary>
        private readonly static ConcurrentDictionary<string, string> ConnectionsMap = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 获取全部在线用户.
        /// </summary>
        /// <returns></returns>
        public static List<ChatUser> GetUsers() => Connections.ToList();
        /// <summary>
        /// 获取某个在线用户.
        /// </summary>
        /// <param name="userId">用户Id from JwtRegisteredClaimNames.Sid</param>
        /// <returns></returns>
        public static List<ChatUser> GetUsers(string userId) => Connections.Where(u => u.Id == userId).ToList();
        /// <summary>
        /// 获取某些在线用户.
        /// </summary>
        /// <param name="userIdList">用户Id from JwtRegisteredClaimNames.Sid</param>
        /// <returns></returns>
        public static List<ChatUser> GetUsers(IEnumerable<string> userIdList) => Connections.Where(u => userIdList.Contains(u.Id)).ToList();
        /// <summary>
        /// 获取聊天室(群)在线用户.
        /// </summary>
        /// <param name="room">聊天室(群) from Query["room"]</param>
        /// <returns></returns>
        public static List<ChatUser> GetRoomUsers(string room) => Connections.Where(u => u.Room == room).ToList();
        /// <summary>
        /// 获取在线用户连接ID.
        /// </summary>
        /// <param name="userId">用户Id from JwtRegisteredClaimNames.Sid</param>
        /// <returns></returns>
        public static List<string> GetConnectionsId(string userId)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(userId)) return list;
            foreach (var item in GetUsers(userId))
            {
                if (!ConnectionsMap.TryGetValue(item.Id + item.Device, out string id)) continue;
                list.Add(id);
            }
            return list;
        }
        /// <summary>
        /// 获取在线用户连接ID.
        /// </summary>
        /// <param name="userIdList">用户Id from JwtRegisteredClaimNames.Sid</param>
        /// <returns></returns>
        public static List<string> GetConnectionsId(IEnumerable<string> userIdList)
        {
            var list = new List<string>();
            if (userIdList == null || !userIdList.Any()) return list;
            foreach (var item in GetUsers(userIdList))
            {
                if (!ConnectionsMap.TryGetValue(item.Id + item.Device, out string id)) continue;
                list.Add(id);
            }
            return list;
        }

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
        /// <param name="room">聊天室(群)</param>
        /// <param name="method">加入+事件方法</param>
        /// <param name="leaveOwnRoom">离开当前聊天室-事件方法</param>
        /// <returns>throw Clients "onError"</returns>
        public async Task Join(string room, string method = "addUser", string leaveOwnRoom = "removeUser")
        {
            try
            {
                var user = Context.GetChatUser();
                if (string.IsNullOrEmpty(user.Id)) return;

                var item = Connections.FirstOrDefault(u => u.Id == user.Id);
                if (item == null || item.Room == room) return;

                // Remove user from others list
                if (!string.IsNullOrEmpty(leaveOwnRoom) && !string.IsNullOrEmpty(user.Room))
                    await Clients.OthersInGroup(user.Room).SendAsync(leaveOwnRoom, user);

                // Join to new chat room
                await Leave(user.Room);
                await Groups.AddToGroupAsync(Context.ConnectionId, room);
                user.Room = room;

                // Tell others to update their list of users
                await Clients.OthersInGroup(room).SendAsync(method, user);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("onError", "Join:" + ex.Message);
            }
        }

        /// <summary>
        /// 发送消息给聊天室的用户
        /// </summary>
        /// <param name="room">聊天室(群)</param>
        /// <param name="userId">用户Id from JwtRegisteredClaimNames.Sid</param>
        /// <param name="message">发送的消息</param>
        /// <param name="method">发送消息+事件方法</param>
        /// <returns></returns>
        public async Task Send(string room, string userId, string message, string method = "newMessage")
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var clients = new List<IClientProxy>();
                if (!string.IsNullOrEmpty(room)) clients.Add(Clients.Group(room));
                if (!string.IsNullOrEmpty(userId)) foreach (var connectionId in GetConnectionsId(userId)) clients.Add(Clients.Client(connectionId));

                // Build the message
                var item = new Message
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", string.Empty),
                    From = Context.GetName(),
                    Time = DateTime.Now,
                };

                // Send the message
                foreach (IClientProxy client in clients) await client.SendAsync(method, item);
                await Clients.Caller.SendAsync(method, item);
            }
        }

        /// <summary>
        /// 离开聊天室
        /// </summary>
        /// <param name="room">聊天室(群)</param>
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
