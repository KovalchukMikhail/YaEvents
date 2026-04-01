using YaEvents.Infrastructure.Enums;

namespace YaEvents.Data.Dto
{
    public record BookingInfo(Guid Id, Guid EventId, BookingStatus Status, DateTime CreatedAt, DateTime? ProcessedAt);
}
