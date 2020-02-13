using System;

namespace ClientApi.Exceptions
{
    public class MalformedAccountException : Exception
    {
        public MalformedAccountException() : base() { }
        public MalformedAccountException(string message) : base(message) { }
        public MalformedAccountException(string message, Exception innerException) : base(message, innerException) { }
    }
}
