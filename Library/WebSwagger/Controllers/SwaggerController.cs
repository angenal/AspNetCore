using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace WebSwagger.Controllers
{
    /// <summary>
    /// Swagger 控制器
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/swagger")]
    public class SwaggerController : Controller
    {
        /// <summary>
        /// Get permissions for the current user.
        /// </summary>
        [HttpGet("getPermissions")]
        public async Task<IActionResult> GetPermissionsAsync()
        {
            var result = new { permissions = Array.Empty<string>() };
            IPermissionChecker checker = HttpContext.RequestServices.GetService<IPermissionChecker>();
            if (checker != null && HttpContext.User?.Identity?.Name != null)
                result = new { permissions = await checker.GetAsync(HttpContext.User.Identity.Name) };
            return Json(result);
        }
    }
}
