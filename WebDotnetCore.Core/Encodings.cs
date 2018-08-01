using System.Text;

namespace WebCore
{
    public static class Encodings
    {
        public static readonly UTF8Encoding Utf8;
        public static readonly UTF8Encoding Utf8_ThrowOnInvalid;

        static Encodings()
        {
            Utf8 = new UTF8Encoding(); // System.Text.Encoding.UTF8
            Utf8_ThrowOnInvalid = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        }
    }
}
