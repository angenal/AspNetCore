using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// 允许具有指定角色或权限的用户可以访问。除非允许匿名访问 AllowAnonymous
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AllowUserAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        /// <summary>
        /// Initializes a new instance of the Microsoft.AspNetCore.Authorization.OperationAttribute class.
        /// </summary>
        public AllowUserAttribute() : base() { }

        /// <summary>
        /// Initializes a new instance of the Microsoft.AspNetCore.Authorization.OperationAttribute class with the specified policy.
        /// </summary>
        /// <param name="policy">The name of the policy to require for authorization.</param>
        public AllowUserAttribute(string policy) : base(policy) { }

        /// <summary>
        /// Initializes a new instance of the Microsoft.AspNetCore.Authorization.OperationAttribute class with the specified roles and permissions.
        /// </summary>
        /// <param name="roles">The list of roles to require for authorization.</param>
        /// <param name="permissions">The list of permissions to require for authorization.</param>
        public AllowUserAttribute(string roles, string permissions) : base()
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
            //if (!(context.ActionDescriptor is ControllerActionDescriptor)) return;
            // 如果权限检查未通过，则返回403禁止访问(用户无权限或账号异常时)
            IPermissionChecker checker = context.HttpContext.RequestServices.GetService<IPermissionChecker>();
            if (checker == null || !checker.AuthorizeAsync(Permissions, context.HttpContext.User.Identity.Name).Result)
                context.Result = new StatusCodeResult(403); //System.Net.HttpStatusCode.Forbidden
        }
    }
}
