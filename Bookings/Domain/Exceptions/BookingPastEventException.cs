using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions
{
    public class BookingPastEventException : Exception
    {
        public BookingPastEventException() { }
        public BookingPastEventException(string message) : base(message) { }
        public BookingPastEventException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
