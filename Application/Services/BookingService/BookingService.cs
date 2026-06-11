using Application.DTO;
using Application.Repositories;
using Application.Semaphores;
using Application.Services.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Services.BookingService
{
    public class BookingService : IBookingService
    {
        protected readonly IBookingsRepository _bookingsRepository;
        protected readonly IEventsRepository _eventRepository;
        public BookingService(IBookingsRepository bookingsRepository, IEventsRepository eventsRepository)
        {
            _bookingsRepository = bookingsRepository;
            _eventRepository = eventsRepository;
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
                    throw new DomainValidationException("Не удалось создать объект бронирования так как объект события помечен как удаленный") { EntityId = eventID };

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
