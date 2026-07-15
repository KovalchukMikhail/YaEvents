using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions
{
    public class NoRightsToOperationException : Exception
    {
        public NoRightsToOperationException() { }
        public NoRightsToOperationException(string message) : base(message) { }
        public NoRightsToOperationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
