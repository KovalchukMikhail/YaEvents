using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Repositories.EventsRepository;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Application.Services.BookingService
{
    public class BookingService : IBookingService
    {

        protected readonly IRepository<Booking> _bookingRepository;
        protected readonly IRepository<Event> _eventRepository;
        public BookingService(IRepository<Booking> repository, IRepository<Event> eventRepository)
        {
            _bookingRepository = repository;
            _eventRepository = eventRepository;
        }
        public async Task<BookingInfo> CreateBookingAsync(Guid eventID, CancellationToken token = default)
        {
            var newBooking = new Booking()
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                EventId = eventID,
                Status = BookingStatus.Pending
            };

            await _bookingRepository.Add(newBooking, token: token);

            return new BookingInfo
            (
                newBooking.Id,
                newBooking.EventId,
                newBooking.Status,
                newBooking.CreatedAt,
                newBooking.ProcessedAt
            );
        }

        public async Task<BookingInfo?> GetBookingByIdAsync(Guid bookingId, CancellationToken token = default)
        {
            var requiredBooking = await _bookingRepository.Get(bookingId, token: token);
            if (requiredBooking != null)
            {
                return new BookingInfo
                (
                    requiredBooking.Id,
                    requiredBooking.EventId,
                    requiredBooking.Status,
                    requiredBooking.CreatedAt,
                    requiredBooking.ProcessedAt
                );
            }
            else
                return null;
        }

        public async Task ProcessBookings(CancellationToken token = default)
        {
            var bookings = await _bookingRepository.GetAll(token);
            foreach (var booking in bookings)
            {
                var curEvent = await _eventRepository.Get(booking.EventId, token);
                if (curEvent == null || curEvent.Status == EventStatus.Removed)
                {
                    if (booking.Status != BookingStatus.Rejected)
                    {
                        await Task.Delay(2000, token);
                        booking.Status = BookingStatus.Rejected;
                        booking.ProcessedAt = DateTime.Now;
                    }
                }
                else if (booking.Status == BookingStatus.Pending)
                {
                    await Task.Delay(2000, token);
                    booking.Status = BookingStatus.Confirmed;
                    booking.ProcessedAt = DateTime.Now;
                }
            }
        }
    }
}
