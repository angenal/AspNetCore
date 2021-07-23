namespace WebInterface.Settings
{
    /// <summary>
    /// Api Client Identity Settings
    /// </summary>
    public class ApiSettings
    {
        /// <summary>
        /// Default Instance.
        /// </summary>
        public static ApiSettings Instance;
        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        public const string AppSettings = "API";
        /*
          "API": {
            "Title": "Demo",
            "Description": "REST API",
            "MaxLengthLimit": 8000,
            "MaxRequestBodySize": 30000000,
            "MaxMultipartBodySize": 134217728,
            "Sid": "e14a7e52-6f07-4372-8927-a1d476733f72",
            "Secret": ""
          }
        */

        /// <summary>
        /// HttpRequest Query Variable for Authorization
        /// </summary>
        public const string HttpRequestQuery = "token";

        /// <summary>
        /// 接口标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 接口描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 提交元素个数限制 (默认 8000)
        /// </summary>
        public int MaxLengthLimit { get; set; } = 8000;
        /// <summary>
        /// 提交数据文本字节数量限制 (默认 28.6 MB)
        /// </summary>
        public int MaxRequestBodySize { get; set; } = 30000000;
        /// <summary>
        /// 上传文件大小限制 (默认 128 MB)
        /// </summary>
        public int MaxMultipartBodySize { get; set; } = 134217728;

        /// <summary>
        /// 第三方接口调用身份标识: X-Request-Sid
        /// </summary>
        public System.Guid Sid { get; set; }
        /// <summary>
        /// 接口数据传输加密的安全秘钥
        /// </summary>
        public string Secret { get; set; }
    }
}
