using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class Booking
    {
        public Guid Id { get; init; }
        public Guid EventId { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Guid UserId { get; set; }

        private Booking() { }

        public Booking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt, DateTime? processedAt, Guid userId)
        {
            Id = id;
            EventId = eventId;
            Status = status;
            CreatedAt = createdAt;
            ProcessedAt = processedAt;
            UserId = userId;
        }

        public bool Confirm()
        {
            if (Status == BookingStatus.Confirmed)
                return false;

            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.Now.ToUniversalTime();
            return true;
        }
        public bool Cancel()
        {
            if (Status == BookingStatus.Cancelled)
                return false;

            Status = BookingStatus.Cancelled;
            ProcessedAt = DateTime.Now.ToUniversalTime();
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
