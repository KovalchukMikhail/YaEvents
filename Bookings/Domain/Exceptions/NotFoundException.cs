using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public Guid? EntityId { get; set; }
        public NotFoundException() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
