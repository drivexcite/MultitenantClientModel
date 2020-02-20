using System;

namespace ClientModel.Exceptions
{
    public class SubscriptionNotFoundException : Exception
    {
        public SubscriptionNotFoundException() : base() { }
        public SubscriptionNotFoundException(string message) : base(message) { }
        public SubscriptionNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
