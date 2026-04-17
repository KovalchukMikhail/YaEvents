using System.ComponentModel.DataAnnotations;
using YaEvents.Infrastructure.Enums;

namespace YaEvents.Data.Dto
{
    public record EventInfo(Guid Id, string? Title, string? Description, DateTime StartAt, DateTime EndAt, EventStatus Status, int TotalSeats, int AvailableSeats);
}
