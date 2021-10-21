using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using WebFramework.Data;

namespace ApiDemo.NET5.Controllers
{
    public partial class ValuesController
    {
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
        [HttpGet("{id}", Name = "GetById")]
        [ProducesResponseType(200)] // Get
        [ProducesResponseType(400)] // BadRequest
        public ActionResult<string> Get(int id)
        {
            return id >= 0 ? context.StringValues.Find(id)?.Value ?? $"查询失败 id = {id}" : BadRequest($"要求 id >= 0, 但是 id = {id}");
        }
    }
}
