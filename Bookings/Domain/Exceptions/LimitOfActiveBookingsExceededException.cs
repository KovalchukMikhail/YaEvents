using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions
{
    public class LimitOfActiveBookingsExceededException : Exception
    {
        public int? CurrentBookingsCount { get; set; }
        public int? LimitOfBookings { get; set; }
        public LimitOfActiveBookingsExceededException() { }
        public LimitOfActiveBookingsExceededException(string message) : base(message) { }
        public LimitOfActiveBookingsExceededException(string message, Exception? innerException) : base(message, innerException) { }
    }
}
