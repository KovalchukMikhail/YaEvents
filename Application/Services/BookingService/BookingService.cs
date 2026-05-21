using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using YaEvents.Application.Services.EventService;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;
using YaEvents.Infrastructure.Repositories.BookingsRepository;
using YaEvents.Infrastructure.Repositories.EventsRepository;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Application.Services.BookingService
{
    public class BookingService : IBookingService
    {
        protected readonly ILogger<BookingService> _logger;
        protected readonly IBookingsRepository _bookingsRepository;
        protected readonly IEventsRepository _eventRepository;
        public BookingService(IBookingsRepository bookingsRepository, IEventsRepository eventsRepository, ILogger<BookingService> logger)
        {
            _bookingsRepository = bookingsRepository;
            _eventRepository = eventsRepository;
            _logger = logger;
        }
        public async Task<BookingInfo> CreateBookingAsync(Guid eventID, CancellationToken token = default)
        {
            var semaphore = AppSemaphores.GetSemaphore(eventID);
            await semaphore.WaitAsync(token);
            Booking? newBooking = null;
            try
            {
                var requiredEvent = await _eventRepository.Get(eventID);
                if (requiredEvent == null)
                    throw new NotFoundException("Не удалось создать объект бронирования так как объект события с указанным Id отсутствует") { EntityId = eventID };
                else if (requiredEvent.Status == EventStatus.Removed)
                    throw new ValidationException("Не удалось создать объект бронирования так как объект события помечен как удаленный") { EntityId = eventID };

                if (!(await _eventRepository.TryReserveSeats(requiredEvent.Id, token)))
                    throw new NoAvailableSeatsException("No available seats for this event") { EntityId = eventID };

                newBooking = new Booking
                (
                    Guid.NewGuid(),
                    eventID,
                    BookingStatus.Pending,
                    DateTime.Now.ToUniversalTime(),
                    null,
                    requiredEvent
                );

                await _bookingsRepository.Add(newBooking, token);
            }
            finally
            {
                semaphore.Release();
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
            var requiredBooking = await _bookingsRepository.Get(bookingId, token);
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


    }
}
