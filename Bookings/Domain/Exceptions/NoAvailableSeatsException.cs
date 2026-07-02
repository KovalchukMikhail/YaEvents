using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions
{
    public class NoAvailableSeatsException : Exception
    {
        public Guid? EntityId { get; set; }
        public NoAvailableSeatsException() { }
        public NoAvailableSeatsException(string message) : base(message) { }
        public NoAvailableSeatsException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
