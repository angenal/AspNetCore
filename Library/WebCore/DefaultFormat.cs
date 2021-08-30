namespace WebCore
{
    public static class DefaultFormat
    {
        public const string CommaChars = ",，";
        public static readonly char[] SeparatorChars = CommaChars.ToCharArray();

        /// <summary>
        /// Sets HTTP request headers["User-Agent"]
        /// </summary>
        public const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.82 Safari/537.36";

        // Gets or sets how System.DateTime and System.DateTimeOffset values are formatted
        // when writing JSON text, and the expected date format when reading JSON text.
        // The default value is "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK".
        public static string DateTimeFormats = "yyyy-MM-dd HH:mm:ss";
        public static string DateFormats = "yyyy-MM-dd";
        public static string TimeFormats = "HH:mm:ss";

        public static string DateTimeFormatsToWrite = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";
        public static string DateTimeOffsetFormatsToWrite = "o";

        public static readonly string[] OnlyDateTimeFormat = {
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
            DateTimeFormatsToWrite,
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff'Z'"
        };

        /// <remarks>
        /// 'r' format is used on the in metadata, because it's delivered as http header.
        /// </remarks>
        public static readonly string[] DateTimeFormatsToRead = {
            DateTimeOffsetFormatsToWrite,
            DateTimeFormatsToWrite,
            "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
            "r",
            "yyyy-MM-ddTHH:mm:ss.fffK",
            "yyyy-MM-ddTHH:mm:ss.FFFK",
        };
    }
}
