using System;

namespace ClientModel.Exceptions
{
    public class MalformedDataLinkException : Exception
    {
        public MalformedDataLinkException() : base() { }
        public MalformedDataLinkException(string message) : base(message) { }
        public MalformedDataLinkException(string message, Exception innerException) : base(message, innerException) { }
    }
}