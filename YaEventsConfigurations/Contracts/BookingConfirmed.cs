using System;
using System.Collections.Generic;
using System.Text;

namespace YaEventsConfigurations.Contracts
{
    public record BookingConfirmed(Guid MessageId, Guid EventId, Guid BookingId, Guid UserId, int CountOfSeats, DateTime? ProcessedAt);
}
