using System;
using System.Collections.Generic;
using System.Text;

namespace ClientModel.Exceptions
{
    public class DataLinkTypeNotFoundException : Exception
    {
        public DataLinkTypeNotFoundException() : base() { }
        public DataLinkTypeNotFoundException(string message) : base(message) { }
        public DataLinkTypeNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
