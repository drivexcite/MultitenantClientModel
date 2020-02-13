using System;

namespace ClientApi.Exceptions
{
    public class MalformedSubscriptionsException : Exception
    {
        public MalformedSubscriptionsException() : base() { }
        public MalformedSubscriptionsException(string message) : base(message) { }
        public MalformedSubscriptionsException(string message, Exception innerException) : base(message, innerException) { }
    }
}
