namespace WebFramework.Settings
{
    /// <summary>
    /// Api Client Identity Settings
    /// </summary>
    public class ApiSettings
    {
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
