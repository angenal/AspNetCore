using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using WebFramework;

namespace ApiDemo.NET5.Controllers
{
    /// <summary>
    /// 演示
    /// </summary>
    [ApiController]
    //[ApiExplorerSettings(GroupName = "app,h5")] //两组Api同时展示
    //[ApiExplorerSettings(GroupName = "demo"), Display(Name = "演示系统", Description = "演示系统描述文字")]
    [Route("api/[controller]/[action]")]
    public partial class PersonalAnswerController : ApiController
    {
        private readonly IWebHostEnvironment env;

        /// <summary></summary>
        public PersonalAnswerController(IWebHostEnvironment env)
        {
            this.env = env;
        }
    }

    #region 输入输出

    /// <summary></summary>
    public class PersonalAnswerModel1
    {
        /// <summary></summary>
        public int Id { get; set; }
        /// <summary></summary>
        public string Ip { get; set; }
        /// <summary></summary>
        public string Title { get; set; }
    }

    #endregion
}
