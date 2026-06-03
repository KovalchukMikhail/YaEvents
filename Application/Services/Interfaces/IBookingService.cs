using Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingInfo> CreateBookingAsync(Guid eventID, CancellationToken token = default);
        Task<BookingInfo?> GetBookingByIdAsync(Guid bookingId, CancellationToken token = default);
    }
}
