using EasyCaching.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WebFramework.Models.DTO;
using WebFramework.SignalR;

namespace WebFramework.Controllers
{
    /// <summary>
    /// 聊天消息
    /// </summary>
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class ChatController : ApiController
    {
        private readonly IWebHostEnvironment env;
        private readonly IRedisCachingProvider redis;
        private readonly IHubContext<ChatHub> hubContext;

        /// <summary></summary>
        public ChatController(IWebHostEnvironment env, IHubContext<ChatHub> hubContext, IEasyCachingProviderFactory cacheFactory)
        {
            this.env = env;
            this.hubContext = hubContext;
            redis = cacheFactory.GetRedisProvider(EasyCachingConstValue.DefaultRedisName);
        }

        /// <summary>
        /// 获取某个在线用户.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<ChatUser>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult GetUser([FromQuery] MessageUserListInputDto input)
        {
            var result = ChatHub.GetUsers(input.UserId);
            return Ok(result ?? new List<ChatUser>());
        }

        /// <summary>
        /// 获取聊天室(群)在线用户.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<ChatUser>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult GetUsers([FromQuery] MessageUsersListInputDto input)
        {
            var result = ChatHub.GetRoomUsers(input.GroupName);
            return Ok(result ?? new List<ChatUser>());
        }

        /// <summary>
        /// 获取聊天室(群)的消息列表.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Message>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult GetMessages([FromQuery] MessageListInputDto input)
        {
            // Gets stored message
            var result = ChatMessage.Get(input.GroupName, input.Size);
            return Ok(result ?? new Message[0]);
        }

        /// <summary>
        /// 发送消息给在线用户.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> SendMessage([FromBody] MessageInputDto input)
        {
            var message = new Message
            {
                Id = Guid.NewGuid().ToString("N"),
                Content = input.Content,
                From = input.From,
                Time = DateTime.Now,
            };

            var clients = new List<IClientProxy>();
            foreach (var connectionId in ChatHub.GetConnectionsId(input.ToUser)) clients.Add(hubContext.Clients.Client(connectionId));

            // Send the message
            foreach (IClientProxy client in clients) await client.SendAsync("newMessage", message);

            return Ok();
        }

        /// <summary>
        /// 发送消息给聊天室(群).
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> SendMessages([FromBody] MessagesInputDto input)
        {
            var message = new Message
            {
                Id = Guid.NewGuid().ToString("N"),
                Content = input.Content,
                From = input.From,
                Time = DateTime.Now,
            };

            // Send the message
            await hubContext.Clients.Group(input.GroupName).SendAsync("newMessage", message);

            // Store the message
            ChatMessage.Add(input.GroupName, message);

            return Ok();
        }

    }
}
