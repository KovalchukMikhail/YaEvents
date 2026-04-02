using YaEvents.Infrastructure.Enums;

namespace YaEvents.Data.Models
{
    public class Event
    {
        public Guid Id { get; init; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public EventStatus Status { get; set; }


        public override bool Equals(object? obj)
        {
                return obj is not null
                    && obj is Event curEvent
                    && Id == curEvent.Id
                    && Title == curEvent.Title
                    && Description == curEvent.Description
                    && StartAt == curEvent.StartAt
                    && EndAt == curEvent.EndAt
                    && Status == curEvent.Status;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Title, Description, StartAt, Status);
        }
    }
}
