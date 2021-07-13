using System;

namespace WebCore.Exceptions
{
    public class InvalidStartOfObjectException : Exception
    {
        public InvalidStartOfObjectException()
        {
        }

        public InvalidStartOfObjectException(string message) : base(message)
        {
        }

        public InvalidStartOfObjectException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
