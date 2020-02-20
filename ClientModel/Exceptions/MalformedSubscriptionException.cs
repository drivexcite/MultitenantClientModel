using System;

namespace ClientModel.Exceptions
{
    public class MalformedSubscriptionException : Exception
    {
        public MalformedSubscriptionException() : base() { }
        public MalformedSubscriptionException(string message) : base(message) { }
        public MalformedSubscriptionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
