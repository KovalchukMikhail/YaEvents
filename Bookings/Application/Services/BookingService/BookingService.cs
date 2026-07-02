using Application.DTO;
using Application.Repositories;
using Application.Semaphores;
using Application.Services.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.BookingService
{
    public class BookingService : IBookingService
    {
        protected readonly IBookingsRepository _bookingsRepository;
        public BookingService(IBookingsRepository bookingsRepository)
        {
            _bookingsRepository = bookingsRepository;
        }
        public async Task<BookingInfo> CreateBookingAsync(Guid eventId, Guid userId, int limitOfBookings, CancellationToken token = default)
        {
            var semaphore = AppSemaphores.GetSemaphore(eventId);
            await semaphore.WaitAsync(token);

            try
            {
                var bookings = await _bookingsRepository.GetBookings(userId, eventId, token);
                if (bookings != null && bookings.Length >= limitOfBookings)
                    throw new LimitOfActiveBookingsExceededException("Превышено число активных бронирований для одного пользователя") { CurrentBookingsCount = bookings.Length, LimitOfBookings = limitOfBookings };

                var newBooking = new Booking
                    (
                        Guid.NewGuid(),
                        eventId,
                        BookingStatus.Pending,
                        DateTime.Now.ToUniversalTime(),
                        null,
                        userId
                    );

                await _bookingsRepository.Add(newBooking, token);

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
            finally
            {
                semaphore.Release();
            }

        }
        public async Task<BookingInfo?> GetBooking(Guid bookingId, Guid userId, bool isRoleAdmin, CancellationToken token = default)
        {
            Booking? requiredBooking = await _bookingsRepository.Get(bookingId, token);

            if (requiredBooking == null)
                throw new NotFoundException("Не удалось найти бронирование с указанным идентификатором") { EntityId = bookingId };

            if(requiredBooking.UserId == userId || isRoleAdmin)
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
        public async Task<bool> CancelBooking(Guid userId, Guid bookingId, bool isRoleAdmin, CancellationToken token = default)
        {
            Booking? requiredBooking = await _bookingsRepository.Get(bookingId, token);

            if (requiredBooking == null)
                throw new NotFoundException("Не удалось найти бронирование с указанным идентификатором") { EntityId = bookingId };

            if (requiredBooking.UserId == userId || isRoleAdmin)
            {
                return await _bookingsRepository.Cancel(bookingId, token);
            }
            else
                throw new NoRightsToOperationException("Нельзя отменять бронирования принадлежащии другому пользователю.");
        }
    }
}
