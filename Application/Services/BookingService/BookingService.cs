using YaEvents.Application.Services.EventService;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;
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
            var requiredEvent = await _eventRepository.Get(eventID, token);

            if (requiredEvent == null)
                throw new NotFoundException("Не удалось создать объект бронирования так как объект события с указанным Id отсутствует") { EntityId = eventID };
            else if(requiredEvent.Status == EventStatus.Removed)
                throw new ValidationException("Не удалось создать объект бронирования так как объект события помечен как удаленный") { EntityId = eventID };
            
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
            foreach (var booking in bookings.Where(b => b.Status == BookingStatus.Pending))
            {
                await Task.Delay(2000, token);
                booking.Status = BookingStatus.Confirmed;
                booking.ProcessedAt = DateTime.Now;
            }
        }
    }
}
