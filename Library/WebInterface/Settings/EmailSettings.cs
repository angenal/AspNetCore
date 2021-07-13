using System.Text;

namespace WebInterface.Settings
{
    /// <summary>
    /// Email Smtp Client Settings
    /// </summary>
    public class SmtpSettings
    {
        /// <summary>
        /// Default Instance.
        /// </summary>
        public static SmtpSettings Instance = new SmtpSettings();
        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        public const string AppSettings = "Email:Smtp";
        /*
          "Email": {
            "Smtp": {
              "Host": "smtp.outlook.com",
              "Port": 587,
              "Username": "",
              "Password": "",
              "TimeOut": 0
            }
          }
        */

        public string Host { get; set; } = "smtp.outlook.com";
        public int Port { get; set; } = 587;
        public string Username { get; set; }
        public string Password { get; set; }

        public string AccessToken { get; set; }
        public bool UseOAuth { get; set; }
        public int TimeOut { get; set; }
        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}
