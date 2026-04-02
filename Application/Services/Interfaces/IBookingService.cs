using YaEvents.Data.Dto;

namespace YaEvents.Application.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingInfo> CreateBookingAsync(Guid eventID, CancellationToken token = default);
        Task<BookingInfo?> GetBookingByIdAsync(Guid bookingId, CancellationToken token = default);
        Task ProcessBookings(CancellationToken token = default);
    }
}
