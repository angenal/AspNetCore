namespace WebCore
{
    public sealed class DefaultFormat : Produces
    {
        public const string CommaChars = ",，";
        public static readonly char[] SeparatorChars = CommaChars.ToCharArray();

        /// <summary>
        /// Sets HTTP request headers["User-Agent"]
        /// </summary>
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4 Safari/537.36";

        // Gets or sets how System.DateTime and System.DateTimeOffset values are formatted
        // when writing JSON text, and the expected date format when reading JSON text.
        // The default value is "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK".
        public static string DateTime = "yyyy-MM-dd HH:mm:ss";
        public static string Date = "yyyy-MM-dd";
        public static string Time = "HH:mm:ss";

        public static string DateTimeToWrite = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";
        public static string DateTimeOffsetToWrite = "o";

        public static readonly string[] OnlyDateTimeFormat = {
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
            DateTimeToWrite,
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff'Z'"
        };

        /// <remarks>
        /// 'r' format is used on the in metadata, because it's delivered as http header.
        /// </remarks>
        public static readonly string[] DateTimeFormatsToRead = {
            DateTimeOffsetToWrite,
            DateTimeToWrite,
            "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
            "r",
            "yyyy-MM-ddTHH:mm:ss.fffK",
            "yyyy-MM-ddTHH:mm:ss.FFFK",
        };
    }

    public class Produces
    {
        public const string JSON = "application/json";
        public const string XML = "application/xml";
    }
}
