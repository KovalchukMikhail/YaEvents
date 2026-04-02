using System.ComponentModel.DataAnnotations;
using YaEvents.Infrastructure.Enums;

namespace YaEvents.Data.Dto
{
    public class BookingDtoLite
    {
        [Required(ErrorMessage = "Идентификатор события обязателен для заполнения.")]
        public Guid EventId { get; set; }
        public required BookingStatus Status { get; set; }
        public required DateTime CreatedAt { get; init; }
        public DateTime? ProcessedAt { get; set; }
    }
}
