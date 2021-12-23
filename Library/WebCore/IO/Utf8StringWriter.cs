using System.IO;
using System.Text;

namespace WebCore.IO
{
    public class Utf8StringWriter: StringWriter
    {
        /// <summary>Gets the encoding. </summary>
        public sealed override Encoding Encoding { get { return Encoding.UTF8; } }
    }
}
