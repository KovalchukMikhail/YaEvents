using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Producer
{
    public interface IBookingsProducer : IDisposable
    {
        public Task ProduceBookingConfirmedAsync(Guid bookingId, Guid eventId, Guid userId, int countOfSeats, CancellationToken token = default);
    }
}
