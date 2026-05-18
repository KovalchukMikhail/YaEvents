using YaEvents.Data.Dto;
using YaEvents.Data.Models;

namespace YaEvents.Application.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingInfo> CreateBookingAsync(Guid eventID, CancellationToken token = default);
        Task<BookingInfo?> GetBookingByIdAsync(Guid bookingId, CancellationToken token = default);
        //Task ProcessBookings(CancellationToken token = default);
        //Task RejectBookingAsync(Booking booking, Event? curEvent, CancellationToken token = default);
    }
}
