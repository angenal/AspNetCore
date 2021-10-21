using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebFramework;
using WebInterface;

namespace ApiDemo.NET5.Controllers
{
    /// <summary>
    /// 用户账号
    /// </summary>
    [ApiController]
    [Authorize]
    //[ApiExplorerSettings(GroupName = "app,h5")] //两组Api同时展示
    //[ApiExplorerSettings(GroupName = "demo"), Display(Name = "演示系统", Description = "演示系统描述文字")]
    [ApiVersion("1.0")]
    [Route("api/[controller]/[action]")]
    //[Route("{culture:culture}/[controller]/[action]")]
    public partial class UserController : ApiController
    {
        private readonly ILiteDb liteDb;
        private readonly ICrypto crypto;
        private readonly IMemoryCache cache;

        /// <summary></summary>
        public UserController(ILiteDb liteDb, ICrypto crypto, IMemoryCache cache)
        {
            this.liteDb = liteDb;
            this.crypto = crypto;
            this.cache = cache;
        }
    }
}
