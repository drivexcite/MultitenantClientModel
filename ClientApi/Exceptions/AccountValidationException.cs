using System;
using System.Collections.Generic;

namespace ClientApi.Exceptions
{
    public class AccountValidationException : Exception
    {
        public List<Exception> InnerExceptions { get; set; } = new List<Exception>();

        public AccountValidationException() : base() { }
        public AccountValidationException(string message) : base(message) { }
        public AccountValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
