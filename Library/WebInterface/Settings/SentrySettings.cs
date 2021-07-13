namespace WebInterface.Settings
{
    public class SentrySettings
    {
        /// <summary>
        /// Default Instance.
        /// </summary>
        public static SentrySettings Instance = new SentrySettings();
        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        public const string AppSettings = "Sentry";
        /*
          "Sentry": {
            "Dsn": "https://0357ef2d9cfd4e77a8fd05599bc385c8@o301489.ingest.sentry.io/5426676",
            "Debug": false,
            "SendDefaultPii": true,
            "AttachStacktrace": true,
            "MaxRequestBodySize": "Small",
            "MinimumBreadcrumbLevel": "Debug",
            "MinimumEventLevel": "Warning",
            "DiagnosticLevel": "Error"
          }
        */


    }
}
