using Microsoft.AspNetCore.Mvc;
using WebFramework;
using WebFramework.Data;

namespace ApiDemo.NET5.Controllers
{
    /// <summary>
    /// 例子接口Values
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public partial class ValuesController : ApiController
    {
        private readonly ValuesDbContext context;

        /// <summary>constructor</summary>
        /// <param name="context">DI DbContext</param>
        public ValuesController(ValuesDbContext context)
        {
            this.context = context;
        }
    }
}
