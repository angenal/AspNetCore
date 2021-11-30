using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace WebApiSwagger
{
    /// <summary>
    /// Swagger扩展
    /// </summary>
    public static partial class SwaggerExtensions
    {
        /// <summary>
        /// 写入Swagger页面
        /// </summary>
        /// <param name="response">Http响应</param>
        /// <param name="page">页面名称</param>
        public static void WriteSwaggerPage(this HttpResponse response, string page)
        {
            var stream = SwaggerDocService.Assembly.GetManifestResourceStream($"WebApiSwagger.Resources.{page}.html");
            if (stream == null) return;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            response.ContentType = "text/html;charset=utf-8";
            response.StatusCode = StatusCodes.Status200OK;
            response.Body.Write(buffer, 0, buffer.Length);
        }
    }
}
