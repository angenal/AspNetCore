using System;

namespace WebCore.LowMemory
{
    public class LowMemoryException : Exception
    {

        public LowMemoryException()
        {
        }

        public LowMemoryException(string message) : base(message)
        {
        }

        public LowMemoryException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}