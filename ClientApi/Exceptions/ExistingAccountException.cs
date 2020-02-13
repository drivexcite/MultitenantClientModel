using System;

namespace ClientApi.Exceptions
{
    public class ExistingAccountException : Exception
    {
        public ExistingAccountException() : base() { }
        public ExistingAccountException(string message) : base(message) { }
        public ExistingAccountException(string message, Exception innerException) : base(message, innerException) { }
    }
}
