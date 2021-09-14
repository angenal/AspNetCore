using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace WebFramework.Filters
{
    /// <summary>
    /// 请求参数验证
    /// </summary>
    public class AsyncRequestValidationFilter : IAsyncActionFilter
    {
        /// <summary>
        /// 启用 FluentValidation
        /// </summary>
        public static bool FluentValidation = true;

        /// <summary></summary>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 启用 FluentValidation 才处理输出
            if (!context.ModelState.IsValid && context.Controller is ApiController)
            {
                context.Result = BadRequestResponse(context);
                return;
            }
            await next();
        }

        /// <summary>
        /// Global Output Status 400 BadRequest with Invalid ModelState
        /// </summary>
        public static IActionResult BadRequestResponse(ActionContext context)
        {
            var x = context.ModelState.Where(x => x.Value.Errors.Any());
            if (!x.Any()) return new BadRequestObjectResult(new ErrorJsonBadRequestResultObject { Title = "请求参数错误!" });
            var errors = x.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(v => v.ErrorMessage.Replace("＆", " ")));
            var result = new BadRequestObjectResult(new ErrorJsonBadRequestResultObject
            {
                Title = x.First().Value.Errors.First().ErrorMessage.Replace("＆", " "),
                Errors = errors.Keys.Select(k => new ErrorJsonBadRequestField { Code = k, Message = errors[k].ToArray() })
                //Errors = string.Join("；", x.Value.Select(v => string.Join("；", v.Errors.Select(e => e.ErrorMessage)))).Replace("＆", " ")
            });
            result.ContentTypes.Add(System.Net.Mime.MediaTypeNames.Application.Json);
            return result;
        }
    }
}
