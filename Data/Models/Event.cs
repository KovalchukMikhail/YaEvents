using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;

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
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        public SemaphoreSlim EventSemaphore { get; } = new(1, 1);

        //public Event(Guid id, string title, string description, DateTime startAt, DateTime endAt, EventStatus status, int totalSeats, int availableSeats)
        //{
        //    Id = id;
        //    Title = title;
        //    Description = description;
        //    StartAt = startAt;
        //    if (endAt < StartAt)
        //        throw new ValidationException($"Дата окончания события (Id = {id}, Title = {Title}) не может быть раньше даты начала события.");
        //
        //    EndAt = endAt;
        //    Status = status;
        //    if (totalSeats < 1)
        //        throw new ValidationException($"Для события (Id = {id}, Title = {Title}) передано некорректное значение общего количества мест {totalSeats}. Значение должно быть больше 0");
        //    TotalSeats = totalSeats;
        //
        //    if (availableSeats < 0)
        //        throw new ValidationException($"Для события (Id = {id}, Title = {Title}) передано некорректное значение доступного количества мест {availableSeats}. Значение должно быть больше или равно 0");
        //    else if (availableSeats > TotalSeats)
        //        throw new ValidationException($"Для события (Id = {id}, Title = {Title}) передано некорректное значение доступного количества мест {availableSeats}. Значение должно быть ");
        //
        //    AvailableSeats = availableSeats;
        //}

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

        public bool TryReserveSeats(int count = 1)
        {
            if (AvailableSeats < count)
                return false;
            else
            {
                AvailableSeats -= count;
                return true;
            }
        }
        public void ReleaseSeats(int count = 1)
        {
            if(count > 0 && AvailableSeats + count <= TotalSeats)
            {
                AvailableSeats += count;
            }
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Title, Description, StartAt, Status);
        }
    }
}
