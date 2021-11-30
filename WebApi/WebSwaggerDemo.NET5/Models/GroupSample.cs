using WebSwagger.Attributes;

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
        [SwaggerApiGroupInfo(Title = "登录模块", Description = "登录相关接口")]
        Login,
        /// <summary>
        /// 测试
        /// </summary>
        [SwaggerApiGroupInfo(Title = "测试模块", Description = "测试相关接口")]
        Test,
        /// <summary>
        /// 案例
        /// </summary>
        [SwaggerApiGroupInfo(Title = "Demo模块", Description = "案例相关接口")]
        Demo
    }
}
