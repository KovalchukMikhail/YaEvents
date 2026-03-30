namespace YaEvents.Infrastructure.Exceptions
{
    public class NotFoundException : Exception
    {
        public int? EntityId { get; set; }
        public NotFoundException() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
