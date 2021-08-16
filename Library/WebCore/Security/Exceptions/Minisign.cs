using System;

namespace WebCore.Security.Exceptions
{
    public class CorruptPrivateKeyException : Exception
    {
        public CorruptPrivateKeyException()
        {
        }

        public CorruptPrivateKeyException(string message)
            : base(message)
        {
        }

        public CorruptPrivateKeyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class CorruptPublicKeyException : Exception
    {
        public CorruptPublicKeyException()
        {
        }

        public CorruptPublicKeyException(string message)
            : base(message)
        {
        }

        public CorruptPublicKeyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class CorruptSignatureException : Exception
    {
        public CorruptSignatureException()
        {
        }

        public CorruptSignatureException(string message)
            : base(message)
        {
        }

        public CorruptSignatureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class FileSizeExceededException : Exception
    {
        public FileSizeExceededException()
        {
        }

        public FileSizeExceededException(string message)
            : base(message)
        {
        }

        public FileSizeExceededException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
