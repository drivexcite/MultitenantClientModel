using System;
using System.Collections.Generic;

namespace ClientModel.Exceptions
{
    public class ClientModelAggregateException : AggregateException
    {
        public ClientModelAggregateException() : base() { }

        public ClientModelAggregateException(IEnumerable<Exception> innerExceptions) : base(innerExceptions) { }
               
        public ClientModelAggregateException(params Exception[] innerExceptions) : base(innerExceptions) { }
               
        public ClientModelAggregateException(string message) : base(message) { }
               
        public ClientModelAggregateException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions) { }
               
        public ClientModelAggregateException(string message, Exception innerException) : base(message, innerException) { }
               
        public ClientModelAggregateException(string message, params Exception[] innerExceptions) : base(message, innerExceptions) { }
    }
}
