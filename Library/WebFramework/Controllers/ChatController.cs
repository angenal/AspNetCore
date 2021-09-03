using EasyCaching.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache cache;
        private readonly IRedisCachingProvider redis;
        private readonly IHubContext<ChatHub> hubContext;

        /// <summary></summary>
        public ChatController(IWebHostEnvironment env, IHubContext<ChatHub> hubContext, IMemoryCache cache, IEasyCachingProviderFactory cacheFactory)
        {
            this.env = env;
            this.cache = cache;
            this.hubContext = hubContext;
            this.redis = cacheFactory.GetRedisProvider(EasyCachingConstValue.DefaultRedisName);
        }


        /// <summary>
        /// Get messages from the group.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Message>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult GetMessage([FromQuery] MessageListInputDto input)
        {
            var result = ChatMessage.Get(input.GroupName, input.Size);
            return Ok(result ?? new Message[0]);
        }

        /// <summary>
        /// Send a message to the group.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> AddMessage([FromBody] MessageInputDto input)
        {
            var message = new Message
            {
                Id = Guid.NewGuid().ToString("N"),
                Content = input.Content,
                From = input.From,
                Time = DateTime.Now,
            };

            await hubContext.Clients.All.SendAsync("newMessage", message);
            ChatMessage.Add(input.GroupName, message);

            return Ok();
        }

    }
}
