using System;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// 自定义操作权限
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class OperationAttribute : AuthorizeAttribute
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
    }
}
