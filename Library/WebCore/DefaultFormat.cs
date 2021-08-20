namespace WebCore
{
    public static class DefaultFormat
    {
        public const string CommaChars = ",，";
        public static readonly char[] SeparatorChars = CommaChars.ToCharArray();

        public const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.82 Safari/537.36";

        public static readonly string DateTimeFormats = "yyyy-MM-dd HH:mm:ss";
        public static readonly string DateFormats = "yyyy-MM-dd";
        public static readonly string TimeFormats = "HH:mm:ss";

        public static readonly string DateTimeOffsetFormatsToWrite = "o";
        public static readonly string DateTimeFormatsToWrite = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff";

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
