
namespace Domain.Exceptions
{
    public class DomainValidationException : Exception
    {
        public Guid? EntityId { get; set; }

        public DomainValidationException() { }
        public DomainValidationException(string message) : base(message) { }
        public DomainValidationException(string message, Exception? innerException) : base(message, innerException) { }
    }
}
