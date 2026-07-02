using Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingInfo> CreateBookingAsync(Guid eventID, Guid userId, int limitOfBookings, CancellationToken token = default);
        Task<BookingInfo?> GetBooking(Guid bookingId, Guid userId, bool isRoleAdmin, CancellationToken token = default);
        Task<bool> CancelBooking(Guid userId, Guid bookingId, bool isRoleAdmin, CancellationToken token = default);
    }
}
