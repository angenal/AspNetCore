using Microsoft.AspNetCore.Mvc;

namespace WebApiSwagger.Internals
{
    /// <summary>
    /// JavaScript结果
    /// </summary>
    public class JsResult : ContentResult
    {
        /// <summary>
        /// 初始化一个<see cref="JsResult"/>类型的实例
        /// </summary>
        /// <param name="script">js</param>
        public JsResult(string script)
        {
            Content = script;
            ContentType = "application/javascript";
        }
    }
}
