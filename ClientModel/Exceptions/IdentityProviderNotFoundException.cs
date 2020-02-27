using System;

namespace ClientModel.Exceptions
{
    public class IdentityProviderNotFoundException : Exception
    {
        public IdentityProviderNotFoundException() : base() { }
        public IdentityProviderNotFoundException(string message) : base(message) { }
        public IdentityProviderNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
