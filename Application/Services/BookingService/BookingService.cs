using Microsoft.AspNetCore.Mvc.Infrastructure;
using YaEvents.Application.Services.EventService;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;
using YaEvents.Infrastructure.Repositories.BookingsRepository;
using YaEvents.Infrastructure.Repositories.EventsRepository;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Application.Services.BookingService
{
    public class BookingService : IBookingService
    {

        protected readonly BookingsRepository _bookingRepository;
        protected readonly IRepository<Event> _eventRepository;
        protected readonly ILogger<BookingService> _logger;
        public BookingService(BookingsRepository repository, IRepository<Event> eventRepository, ILogger<BookingService> logger)
        {
            _bookingRepository = repository;
            _eventRepository = eventRepository;
            _logger = logger;
        }
        public async Task<BookingInfo> CreateBookingAsync(Guid eventID, CancellationToken token = default)
        {
            var requiredEvent = await _eventRepository.Get(eventID, token);
            if (requiredEvent == null)
                throw new NotFoundException("Не удалось создать объект бронирования так как объект события с указанным Id отсутствует") { EntityId = eventID };
            else if (requiredEvent.Status == EventStatus.Removed)
                throw new ValidationException("Не удалось создать объект бронирования так как объект события помечен как удаленный") { EntityId = eventID };

            await requiredEvent.EventSemaphore.WaitAsync();
            Booking newBooking = null;
            try
            {
                if(!requiredEvent.TryReserveSeats())
                    throw new NoAvailableSeatsException("No available seats for this event") { EntityId = eventID };

                await _eventRepository.Update(requiredEvent);
                newBooking = new Booking()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    EventId = eventID,
                    Status = BookingStatus.Pending
                };

                await _bookingRepository.Add(newBooking, token: token);
            }
            finally
            {
                requiredEvent.EventSemaphore.Release();
            }

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
            var pendingBookings = await _bookingRepository.GetPending(token);
            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, token)).ToList();

            await Task.WhenAll(tasks);
        }
        public async Task ProcessBookingAsync(Booking booking, CancellationToken token = default)
        {
            var requiredEventTask = _eventRepository.Get(booking.EventId, token);

            await Task.WhenAll(requiredEventTask, Task.Delay(2000, token));
            var requiredEvent = requiredEventTask.Result;

            _logger.LogInformation("Обрабатывается бронирование Id = {id}", booking.Id);

            if(requiredEvent == null)
            {
                await RejectBookingAsync(booking, requiredEvent);
                throw new ValidationException("Не удалось обработать объект бронирования так как объект события отсутствует") { EntityId = booking.Id };
            }
            else if (requiredEvent.Status == EventStatus.Removed)
            {
                await RejectBookingAsync(booking, requiredEvent);
                throw new ValidationException("Не удалось обработать объект бронирования так как объект события помечен как удаленный") { EntityId = booking.Id };
            }

            await booking.BookingSemaphore.WaitAsync();
            try
            {
                booking.Confirm();
                await _bookingRepository.Update(booking);
            }
            finally
            {
                booking.BookingSemaphore.Release();
            }
        }

        public async Task RejectBookingAsync(Booking booking, Event? curEvent, CancellationToken token = default)
        {
            if (curEvent == null || curEvent.Id != booking.EventId)
            {
                curEvent = await _eventRepository.Get(booking.EventId, token);
            }
            
            await booking.BookingSemaphore.WaitAsync();
            try
            {
                if(booking.Reject())
                {
                    if (curEvent != null)
                    {
                        await curEvent.EventSemaphore.WaitAsync();
                        try
                        {
                            curEvent.ReleaseSeats();
                            await Task.WhenAll(_bookingRepository.Update(booking), _eventRepository.Update(curEvent));
                        }
                        finally
                        {
                            curEvent.EventSemaphore.Release();
                        }
                    }
                    else
                    {
                        await _bookingRepository.Update(booking);
                    }
                }

            }
            finally
            {
                booking.BookingSemaphore.Release();
            }
        }
    }
}
