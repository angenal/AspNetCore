using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using WebFramework;

namespace ApiDemo.NET5.Controllers
{
    /// <summary>
    /// 例子接口Values
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ApiController
    {
        private readonly ValuesDbContext context;

        /// <summary> constructor </summary>
        /// <param name="context">DI DbContext</param>
        public ValuesController(ValuesDbContext context)
        {
            this.context = context;
        }

        // GET api/values
        /// <summary>
        /// 查询 - Values
        /// </summary>
        /// <param name="init"></param>
        /// <returns></returns>
        [HttpGet("{init=0}", Name = "GetAll")]
        public ActionResult<IEnumerable<string>> Get([FromQuery] string init)
        {
            var values = context.StringValues.ToArray();
            if (values.Length == 0 && init == "1")
            {
                context.StringValues.AddRange(
                    new ValueModel { Id = 1, Value = "Item1" },
                    new ValueModel { Id = 2, Value = "Item2" },
                    new ValueModel { Id = 3, Value = "Item3" },
                    new ValueModel { Id = 4, Value = "Item4" },
                    new ValueModel { Id = 5, Value = "Item5" },
                    new ValueModel { Id = 6, Value = "Item6" },
                    new ValueModel { Id = 7, Value = "Item7" },
                    new ValueModel { Id = 8, Value = "Item8" },
                    new ValueModel { Id = 9, Value = "Item9" }
                );
                context.SaveChanges();
            }
            return values.Select(o => o.Value).ToArray();
        }

        // GET api/values/5
        /// <summary>
        /// 查询 - Value
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType(200)] // Get
        [ProducesResponseType(400)] // BadRequest
        [HttpGet("{id}", Name = "GetById")]
        public ActionResult<string> Get(int id)
        {
            string value = "";
            if (id >= 0)
            {
                value = context.StringValues.Find(id)?.Value ?? $"查询失败 id = {id}";
            }
            else
            {
                return BadRequest($"要求 id >= 0, 但是 id = {id}");
            }
            return value;
        }

        // POST api/values
        /// <summary>
        /// 新增 - Value
        /// </summary>
        /// <param name="value"></param>
        [ProducesResponseType(typeof(string), 201)] // Created : 返回IActionResult时需要指定ProducesResponseType
        [ProducesResponseType(400)] // BadRequest
        [HttpPost(Name = "Create")]
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

        // PUT api/values/5
        /// <summary>
        /// 更新 - Value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("{id}", Name = "Update")]
        public ActionResult<int> Put(int id, [FromBody] string value)
        {
            int effected = 0;
            if (id >= 0 && id < context.StringValues.Count())
            {
                var model = context.StringValues.Find(id);
                if (model != null)
                {
                    model.Value = value;
                    effected = context.SaveChanges();
                }
                else
                {
                    effected = -1;
                }
            }
            return effected;
        }

        // DELETE api/values/5
        /// <summary>
        /// 删除 - Value
        /// </summary>
        /// <param name="id"></param>
        [Produces(typeof(int))] // 推断 new ObjectResult(int) => ActionResult<int>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            int effected = 0;
            if (id >= 0 && id < context.StringValues.Count())
            {
                var value = context.StringValues.Find(id);
                if (value != null)
                {
                    context.StringValues.Remove(value);
                    effected = context.SaveChanges();
                }
                else
                {
                    effected = -1;
                }
            }
            return new ObjectResult(effected);
        }
    }
}
