using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// 自定义操作权限
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class OperationAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        /// <summary>
        /// Initializes a new instance of the Microsoft.AspNetCore.Authorization.OperationAttribute class.
        /// </summary>
        public OperationAttribute() : base() { }

        /// <summary>
        /// Initializes a new instance of the Microsoft.AspNetCore.Authorization.OperationAttribute class with the specified policy.
        /// </summary>
        /// <param name="policy">The name of the policy to require for authorization.</param>
        public OperationAttribute(string policy) : base(policy) { }

        /// <summary>
        /// Initializes a new instance of the Microsoft.AspNetCore.Authorization.OperationAttribute class with the specified roles and permissions.
        /// </summary>
        /// <param name="roles">The list of roles to require for authorization.</param>
        /// <param name="permissions">The list of permissions to require for authorization.</param>
        public OperationAttribute(string roles, string permissions) : base()
        {
            Roles = roles;
            Permissions = permissions;
        }

        /// <summary>
        /// Gets or sets a comma delimited list of permissions that are allowed to access the resource.
        /// </summary>
        public string Permissions { get; set; }

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized.
        /// </summary>
        /// <param name="context">The Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //// 如果不是一个控制器方法，则直接返回
            //if (!(context.ActionDescriptor is ControllerActionDescriptor))
            //    return;
            // 如果不存在权限检查服务，则直接返回
            IPermissionChecker permissionChecker = context.HttpContext.RequestServices.GetService<IPermissionChecker>();
            if (permissionChecker == null)
                return;
            // 如果权限检查未通过，则返回403(未授权)
            if (!permissionChecker.AuthorizeAsync(Permissions, context.HttpContext.User.Identity.Name).Result)
                context.Result = new ContentResult() { StatusCode = (int)System.Net.HttpStatusCode.Forbidden };
        }
    }
}
