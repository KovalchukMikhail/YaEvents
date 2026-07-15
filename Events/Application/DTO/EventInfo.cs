using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO
{
    public record EventInfo(Guid Id, string? Title, string? Description, DateTime StartAt, DateTime EndAt, EventStatus Status, int TotalSeats, int AvailableSeats);
}
