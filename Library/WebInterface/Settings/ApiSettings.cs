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
        public static ApiSettings Instance = new ApiSettings();
        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        public const string AppSettings = "API";
        /*
          "API": {
            "Title": "Demo",
            "Description": "REST API",
            "Sid": "e14a7e52-6f07-4372-8927-a1d476733f72",
            "Secret": ""
          }
        */

        /// <summary>
        /// HttpRequest Query Variable
        /// </summary>
        public const string HttpRequestQuery = "token";

        /// <summary>
        /// The title of the application.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// A short description of the application.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Request Headers: X-Request-Sid
        /// </summary>
        public System.Guid Sid { get; set; }
        /// <summary>
        /// Back end encryption verification key
        /// </summary>
        public string Secret { get; set; }
    }
}
