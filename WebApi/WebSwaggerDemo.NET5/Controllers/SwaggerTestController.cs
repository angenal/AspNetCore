using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebSwagger.Attributes;
using WebSwaggerDemo.NET5.Models;

namespace WebSwaggerDemo.NET5.Controllers
{
    /// <summary>
    /// Swagger测试信息
    /// </summary>
    [ApiController]
    [SwaggerApiGroup(GroupSample.Test)]
    [Route("api/[controller]")]
    public class SwaggerTestController: Controller
    {
        /// <summary>
        /// 获取所有信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Bing1", "Bing2" };
        }

        /// <summary>
        /// 根据ID获取信息
        /// </summary>
        /// <param name="id">系统编号</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return $"Bing-{id}";
        }

        /// <summary>
        /// Post一个数据信息
        /// </summary>
        /// <param name="value">值</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// 根据ID put数据
        /// </summary>
        /// <param name="id">系统编号</param>
        /// <param name="value">值</param>
        [HttpPut]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// 根据ID删除数据
        /// </summary>
        /// <param name="id">系统编号</param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        /// <summary>
        /// 复杂数据操作
        /// </summary>
        /// <param name="info">样例信息</param>
        /// <returns></returns>
        [HttpPost("test")]
        public SampleNameValue Test(SampleNameValue info)
        {
            return info;
        }

        /// <summary>
        /// 复杂数据操作2
        /// </summary>
        /// <param name="info">样例信息</param>
        /// <returns></returns>
        [HttpPost("test2")]
        public SampleNameValue Test2([FromBody] SampleNameValue info)
        {
            return info;
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost("testUpload")]
        [SwaggerUpload]
        public IActionResult TestUpload()
        {
            return new JsonResult("成功操作");
        }

        /// <summary>
        /// 授权信息
        /// </summary>
        /// <returns></returns>
        [HttpPost("testAuthorizeInfo")]
        //[Authorize("Admin")]
        public IActionResult TestAuthorizeInfo()
        {
            return new JsonResult("成功操作");
        }

        /// <summary>
        /// 请求头
        /// </summary>
        /// <returns></returns>
        [HttpPost("testRequesHeader")]
        [SwaggerRequestHeader("Refrence", "引用")]
        public IActionResult TestRequestHeader()
        {
            return new JsonResult("成功操作");
        }

        /// <summary>
        /// 响应头
        /// </summary>
        /// <returns></returns>
        [HttpPost("testResponseHeader")]
        [SwaggerResponseHeader(200, "正常", "tt", "成功响应")]
        public IActionResult TestResponseHeader()
        {
            return new JsonResult("成功操作");
        }

        /// <summary>
        /// 测试 Post Json属性
        /// </summary>
        /// <param name="request">请求</param>
        [HttpPost("testPostJsonProperty")]
        public IActionResult TestPostJsonProperty([FromBody] PostJsonPropertyRequest request)
        {
            return new JsonResult("成功操作");
        }
    }

    /// <summary>
    /// 名称-值 样例
    /// </summary>
    public class SampleNameValue
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; } = "100";
    }
}
