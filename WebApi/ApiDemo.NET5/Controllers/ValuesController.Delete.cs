using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ApiDemo.NET5.Controllers
{
    public partial class ValuesController
    {
        // DELETE api/values/5
        /// <summary>
        /// 删除 - Value
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        [Produces(typeof(int))] // 推断 new ObjectResult(int) => ActionResult<int>
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
