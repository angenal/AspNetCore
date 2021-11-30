using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebSwagger.Internals;

namespace WebSwagger.Filters.Documents
{
    /// <summary>
    /// 小写Url 文档过滤器
    /// </summary>
    public class LowerUrlDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// 重写操作处理
        /// </summary>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) => swaggerDoc.Paths =
            UrlConvert.ConvertKeys(swaggerDoc.Paths, UrlConvert.Lower);
    }
}
