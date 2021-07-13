using System;

namespace WebCore.Exceptions
{
    public class StreamDisposedException : Exception
    {
        public StreamDisposedException(string message) : base(message)
        {
        }
    }
}
