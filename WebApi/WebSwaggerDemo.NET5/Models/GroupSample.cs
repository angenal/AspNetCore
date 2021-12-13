using System.ComponentModel.DataAnnotations;

namespace WebSwaggerDemo.NET5.Models
{
    /// <summary>
    /// 分组例子
    /// </summary>
    public enum GroupSample
    {
        /// <summary>
        /// 登录
        /// </summary>
        [Display(Name = "登录模块", Description = "登录相关接口")]
        Login,
        /// <summary>
        /// 测试
        /// </summary>
        [Display(Name = "测试模块", Description = "测试相关接口")]
        Test,
        /// <summary>
        /// 案例
        /// </summary>
        [Display(Name = "Demo模块", Description = "案例相关接口")]
        Demo
    }
}
