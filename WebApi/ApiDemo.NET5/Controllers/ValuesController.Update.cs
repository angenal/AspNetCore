using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ApiDemo.NET5.Controllers
{
    public partial class ValuesController
    {
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
    }
}
