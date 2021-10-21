using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WebFramework.Data;

namespace ApiDemo.NET5.Controllers
{
    public partial class ValuesController
    {
        // POST api/values
        /// <summary>
        /// 新增 - Value
        /// </summary>
        /// <param name="value"></param>
        [HttpPost(Name = "Create")]
        [ProducesResponseType(typeof(string), 201)] // Created : 返回IActionResult时需要指定ProducesResponseType
        [ProducesResponseType(400)] // BadRequest
        public IActionResult Post([FromBody] string value)
        {
            int id = 1 + context.StringValues.Max(x => x.Id);
            if (!string.IsNullOrWhiteSpace(value))
            {
                context.StringValues.Add(new ValueModel { Id = id, Value = value });
                context.SaveChanges();
            }
            else
            {
                return BadRequest($"参数 value 不能为空");
            }
            return CreatedAtAction("GetById", new { id }, null);// 201  响应
        }
    }
}
