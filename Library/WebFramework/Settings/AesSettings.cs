namespace WebFramework.Settings
{
    /// <summary>
    /// AES Encryption Settings
    /// </summary>
    public class AesSettings
    {
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
