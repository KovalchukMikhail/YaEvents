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
        protected readonly IUsersRepository _usersRepository;
        public BookingService(IBookingsRepository bookingsRepository, IEventsRepository eventsRepository, IUsersRepository usersRepository)
        {
            _bookingsRepository = bookingsRepository;
            _eventRepository = eventsRepository;
            _usersRepository = usersRepository;
        }
        public async Task<BookingInfo> CreateBookingAsync(Guid eventId, Guid userId, int limitOfBookings, CancellationToken token = default)
        {
            var semaphore = AppSemaphores.GetSemaphore(eventId);
            await semaphore.WaitAsync(token);
            Booking? newBooking = null;
            try
            {
                var requiredEvent = await _eventRepository.Get(eventId, token);
                if (requiredEvent == null)
                    throw new NotFoundException("Не удалось создать объект бронирования так как объект события с указанным Id отсутствует") { EntityId = eventId };
                else if (requiredEvent.Status == EventStatus.Removed)
                    throw new DomainValidationException("Не удалось создать объект бронирования так как объект события помечен как удаленный") { EntityId = eventId };
                else if(requiredEvent.StartAt <= DateTime.Now.ToUniversalTime())
                    throw new BookingPastEventException("Не удалось создать объект бронирования так как событие уже началось");

                var user = await _usersRepository.Get(userId, token);

                if (user == null)
                    throw new NotFoundException("Не удалось создать объект бронирования так как объект пользователя с указанным Id отсутствует") { EntityId = userId };

                if (user.Bookings != null)
                {
                    var bookingsCount = user.Bookings.Where(b => b.Event != null && b.EventId == eventId && b.Status != BookingStatus.Cancelled).Count();
                    if(bookingsCount >= limitOfBookings)
                        throw new LimitOfActiveBookingsExceededException("Превышено число активных бронирований для одного пользователя") { CurrentBookingsCount = bookingsCount, LimitOfBookings = limitOfBookings };
                }

                if (!(await _eventRepository.TryReserveSeats(requiredEvent.Id, token)))
                    throw new NoAvailableSeatsException("No available seats for this event") { EntityId = eventId };

                newBooking = new Booking
                (
                    Guid.NewGuid(),
                    eventId,
                    BookingStatus.Pending,
                    DateTime.Now.ToUniversalTime(),
                    null,
                    requiredEvent,
                    userId,
                    user
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
                newBooking.ProcessedAt,
                newBooking.UserId
            );
        }
        public async Task<BookingInfo?> GetBooking(Guid userId, Guid bookingId, CancellationToken token = default)
        {
            var user = await _usersRepository.Get(userId, token);
            if (user == null)
                throw new NotFoundException("Не удалось найти пользователя с указанным идентификатором") { EntityId = userId };

            Booking? requiredBooking = await _bookingsRepository.Get(bookingId, token);
            if (requiredBooking == null)
                throw new NotFoundException("Не удалось найти бронирование с указанным идентификатором") { EntityId = bookingId };

            if (user.Role == UserRole.Admin || requiredBooking.User!.Id == user.Id)
            {
                return new BookingInfo
                    (
                        requiredBooking.Id,
                        requiredBooking.EventId,
                        requiredBooking.Status,
                        requiredBooking.CreatedAt,
                        requiredBooking.ProcessedAt,
                        requiredBooking.UserId
                    );
            }
            else
                throw new NoRightsToOperationException("Нельзя получить бронирования принадлежащии другому пользователю.");
        }
        public async Task<bool> CancelBooking(Guid userId, Guid bookingId, CancellationToken token = default)
        {
            var user = await _usersRepository.Get(userId, token);
            if (user == null)
                throw new NotFoundException("Не удалось найти пользователя с указанным идентификатором") { EntityId = userId };

            Booking? requiredBooking = await _bookingsRepository.Get(bookingId, token);
            if (requiredBooking == null)
                throw new NotFoundException("Не удалось найти бронирование с указанным идентификатором") { EntityId = bookingId };

            if (user.Role == UserRole.Admin || requiredBooking.User!.Id == user.Id)
            {
                return await _bookingsRepository.Cancel(bookingId, token);
            }
            else
                throw new NoRightsToOperationException("Нельзя отменять бронирования принадлежащии другому пользователю.");
        }
    }
}
