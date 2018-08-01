using System;

namespace WebCore.Platform
{
    public class IncorrectDllException : Exception
    {
        public IncorrectDllException()
        {
        }

        public IncorrectDllException(string message) : base(message)
        {
        }

        public IncorrectDllException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}