using System;

namespace WebInterface.Settings
{
    /// <summary>
    ///
    /// </summary>
    public class CertSettings
    {
    }

    /// <summary>
    /// the certificate information
    /// </summary>
    public class CertInfo
    {
        /// <summary>
        /// Gets the subject distinguished name from the certificate.
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Gets the name of the certificate authority that issued the X.509v3 certificate.
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// Gets the certificate status
        /// </summary>
        public CertStatus Status { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string HashAlgorithm { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string DigestAlgorithm { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string SignatureAlgorithm { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string SigningReason { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string SigningLocation { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string SignatureName { get; set; }
        /// <summary>
        ///
        /// </summary>
        public DateTime SignatureTime { get; set; }
        /// <summary>
        /// Gets the date in local time after which a certificate is no longer valid.
        /// </summary>
        public DateTime SignatureExpireTime { get; set; }
        /// <summary>
        ///
        /// </summary>
        public bool SignatureIsTimestamped { get; set; }
        /// <summary>
        /// Signature is valid?
        /// </summary>
        public bool SignatureIsValid { get; set; }
        /// <summary>
        /// Certificate timestamp info
        /// </summary>
        public CertTimestampInfo TimestampInfo { get; set; }
    }
    /// <summary>
    /// Certificate timestamp info
    /// </summary>
    public class CertTimestampInfo
    {
        /// <summary>
        ///
        /// </summary>
        public string HashAlgorithm { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        ///
        /// </summary>
        public bool IsTimestampAltered { get; set; }
    }
    /// <summary>
    /// Certificate status
    /// </summary>
    public enum CertStatus
    {
        /// <summary>
        ///
        /// </summary>
        Valid = 0,
        /// <summary>
        ///
        /// </summary>
        Revoked = 1,
        /// <summary>
        ///
        /// </summary>
        Expired = 2,
        /// <summary>
        ///
        /// </summary>
        Unknown = 3,
        /// <summary>
        ///
        /// </summary>
        NotPresent = 4
    }
}
