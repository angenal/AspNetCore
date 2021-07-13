namespace WebInterface.Settings
{
    /// <summary>
    /// AES Encryption Settings
    /// </summary>
    public class AesSettings
    {
        /// <summary>
        /// Default Instance.
        /// </summary>
        public static AesSettings Instance = new AesSettings();
        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        public const string AppSettings = "AES";
        /*
          "AES": {
            "Key": "TmIhgugCGFpU7S3v",
            "IV": "jkE49230Tf093b42",
            "Salt": "hgt!16kl"
          }
        */

        /// <summary>
        /// AES key used for the symmetric encryption.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// AES Initialization Vector used for the symmetric encryption.
        /// </summary>
        public string IV { get; set; }
        /// <summary>
        /// Salt for password.
        /// </summary>
        public string Salt { get; set; }
    }
}
