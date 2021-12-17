using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebInterface.Settings;

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
        [HttpGet("getRolePermissions")]
        public async Task<IActionResult> GetRolePermissionsAsync()
        {
            var result = new { id = "", role = "", permissions = Array.Empty<string>() };
            IPermissionChecker checker = HttpContext.RequestServices.GetService<IPermissionChecker>();
            if (checker != null && HttpContext.User?.Identity?.Name != null)
                result = new
                {
                    id = HttpContext.User.Identity.Name,
                    role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtSettings.RoleClaimType)?.Value,
                    permissions = await checker.GetAsync(HttpContext.User.Identity.Name)
                };
            return Json(result);
        }
    }
}
