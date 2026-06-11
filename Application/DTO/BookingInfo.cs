using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO
{
    public record BookingInfo(Guid Id, Guid EventId, BookingStatus Status, DateTime CreatedAt, DateTime? ProcessedAt);
}
