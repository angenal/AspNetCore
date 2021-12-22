using System;
using System.IO;

namespace WebCore.Crawler
{
    public class AbotX
    {
        /// <summary>
        /// AbotX.lic base64 string.
        /// </summary>
        const string License = "PExpY2Vuc2U+DQogIDxMaWNlbnNlQXR0cmlidXRlcz4NCiAgICA8QXR0cmlidXRlIG5hbWU9IlByb2R1Y3ROYW1lIj5BYm90WHYxPC9BdHRyaWJ1dGU+DQogICAgPEF0dHJpYnV0ZSBuYW1lPSJMaWNlbnNlIj5VbHRpbWF0ZTwvQXR0cmlidXRlPg0KICA8L0xpY2Vuc2VBdHRyaWJ1dGVzPg0KICA8SWQ+YzA1Mzg0NjMtYTkwOS00MDg4LTljYWQtMGFkMThkYTBjODdhPC9JZD4NCiAgPFByb2R1Y3RGZWF0dXJlcyAvPg0KICA8Q3VzdG9tZXI+DQogICAgPE5hbWU+Qm9hcmQ0QWxsPC9OYW1lPg0KICAgIDxFbWFpbD5iNGFAYm9hcmQ0YWxsLmJpejwvRW1haWw+DQogIDwvQ3VzdG9tZXI+DQogIDxUeXBlPlN0YW5kYXJkPC9UeXBlPg0KICA8RXhwaXJhdGlvbj5XZWQsIDMxIERlYyAyMTIxIDA4OjU2OjAxIEdNVDwvRXhwaXJhdGlvbj4NCiAgPFNpZ25hdHVyZT5NRVVDSVFEb3gvcGNXY0lEZjZiR1kzOHQ4d0pXbkphSzVsOVREN2xYVFdqdmJYOWdMZ0lnZitlL1N6R001ZjFqTlVBREVMSEUxdG9PeXgydTlUS0k4cE9lM3JRU29Mdz08L1NpZ25hdHVyZT4NCjwvTGljZW5zZT4=";

        public static void Load()
        {
            //new AbotX2.License().SetLicense(GetLicense(License));
        }

        /// <summary>
        /// Read from base64String license.
        /// </summary>
        static Stream GetLicense(string base64String) => new MemoryStream(Convert.FromBase64String(base64String));
    }
}
