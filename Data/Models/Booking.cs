using YaEvents.Infrastructure.Enums;
using static System.Net.WebRequestMethods;

namespace YaEvents.Data.Models
{
    public class Booking
    {
        public Guid Id { get; init; }
        public Guid EventId { get; set; }
        public required BookingStatus Status { get; set; }
        public required DateTime CreatedAt { get; init; }
        public DateTime? ProcessedAt { get; set; }
        public SemaphoreSlim BookingSemaphore { get; } = new(1, 1);

        public bool Confirm()
        {
            if(Status == BookingStatus.Confirmed)
                return false;

            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.Now;
            return true;
        }
        public bool Reject()
        {
            if (Status == BookingStatus.Rejected)
                return false;

            Status = BookingStatus.Rejected;
            ProcessedAt = DateTime.Now;
            return true;
        }
        public override bool Equals(object? obj)
        {
            return obj is not null
                && obj is Booking booking
                && Id == booking.Id
                && EventId == booking.EventId
                && Status == booking.Status
                && CreatedAt == booking.CreatedAt
                && ProcessedAt == booking.ProcessedAt;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, EventId, Status, CreatedAt, ProcessedAt);
        }
    }
}
