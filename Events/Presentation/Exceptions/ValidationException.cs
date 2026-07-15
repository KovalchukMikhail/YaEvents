using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Presentation.Exceptions
{
    public class ValidationException : Exception
    {
        public ModelStateDictionary? ModelState { get; set; }
        public Guid? EntityId { get; set; }

        public ValidationException() { }
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception? innerException) : base(message, innerException) { }
    }
}
