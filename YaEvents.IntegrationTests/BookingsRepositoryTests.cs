using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Repositories.BookingsRepository;

namespace YaEvents.IntegrationTests
{
    [Collection("Database")]
    public class BookingsRepositoryTests
    {
        private readonly DbWorker _dbWorker;
        public BookingsRepositoryTests(DbWorker dbWorker)
        {
            _dbWorker = dbWorker;
        }
        private Booking CreateBooking(Event @event, BookingStatus? status = null)
        {
            return new Booking(Guid.NewGuid(),
                                @event.Id,
                                status ?? BookingStatus.Pending,
                                DateTime.Now.ToUniversalTime(),
                                null,
                                null
                                );
        }
        private Event CreateEvent(string? title = null, string? Description = null, DateTime? startAt = null, DateTime? endAt = null, EventStatus? status = null, int? totalSeats = null, int? availableSeats = null)
        {
            return new Event
                (
                    Guid.NewGuid(),
                    title ?? "Title",
                    Description ?? "Description",
                    startAt ?? DateTime.Parse("2010.01.01").ToUniversalTime(),
                    endAt ?? DateTime.Parse("2011.01.01").ToUniversalTime(),
                    status ?? Infrastructure.Enums.EventStatus.Existing,
                    totalSeats ?? 4,
                    availableSeats ?? 4
                );
        }

        [Fact]
        public async Task Add_CorrectParameters_SaveBookingToDataBase()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var context = await _dbWorker.CreateContext();
            var @event = CreateEvent();
            await context.Events.AddAsync(@event);
            await context.SaveChangesAsync();

            var booking = CreateBooking(@event);

            //Act
            context = await _dbWorker.CreateContext();
            var bookingRepository = new BookingsRepository(context);
            await bookingRepository.Add(booking);

            //Assert
            context = await _dbWorker.CreateContext();
            var reqieredBooking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);
            Assert.NotNull(reqieredBooking);
        }
        [Fact]
        public async Task Get_ExistingId_ReturnsBooking()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var context = await _dbWorker.CreateContext();
            var @event = CreateEvent();
            var booking = CreateBooking(@event);
            await context.Events.AddAsync(@event);
            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();
        
        
            //Act
            context = await _dbWorker.CreateContext();
            var bookingRepository = new BookingsRepository(context);
            var reqieredBooking = await bookingRepository.Get(booking.Id);
        
            //Assert
            Assert.NotNull(reqieredBooking);
        }
        [Fact]
        public async Task GetPending_ReturnsBookingsWithStatusPending()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var context = await _dbWorker.CreateContext();
            var @event = CreateEvent();
            var booking1 = CreateBooking(@event);
            var booking2 = CreateBooking(@event);
            var booking3 = CreateBooking(@event, BookingStatus.Confirmed);
            await context.Events.AddAsync(@event);
            await context.Bookings.AddRangeAsync(booking1, booking2, booking3);
            await context.SaveChangesAsync();
        
        
            //Act
            context = await _dbWorker.CreateContext();
            var bookingRepository = new BookingsRepository(context);
            var reqieredBookings = await bookingRepository.GetPending();
        
            //Assert
            Assert.True(reqieredBookings.All(b => b.Status == BookingStatus.Pending));
        }
        [Fact]
        public async Task Reject_CorrectParameters_ChangeBookingStatusInDataBase()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var context = await _dbWorker.CreateContext();
            var @event = CreateEvent();
            var booking = CreateBooking(@event);
            await context.Events.AddAsync(@event);
            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();
        
        
            //Act
            context = await _dbWorker.CreateContext();
            var bookingRepository = new BookingsRepository(context);
            await bookingRepository.Reject(booking.Id);
        
            //Assert
            context = await _dbWorker.CreateContext();
            var reqieredBooking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);
            Assert.NotNull(reqieredBooking);
            Assert.Equal(BookingStatus.Rejected, reqieredBooking.Status);
        }
        [Fact]
        public async Task GetByEventId_ReturnsOnlyReqieredBookings()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var context = await _dbWorker.CreateContext();
            var event1 = CreateEvent();
            var event2 = CreateEvent();
            var booking1 = CreateBooking(event1);
            var booking2 = CreateBooking(event1);
            var booking3 = CreateBooking(event2, BookingStatus.Confirmed);
            await context.Events.AddRangeAsync(event1, event2);
            await context.Bookings.AddRangeAsync(booking1, booking2, booking3);
            await context.SaveChangesAsync();


            //Act
            context = await _dbWorker.CreateContext();
            var reqieredBookings = await context.Bookings.Where(b => b.EventId == event1.Id).ToArrayAsync();

            //Assert
            Assert.Equal(2, reqieredBookings.Length);
            Assert.True(reqieredBookings.All(b => b.EventId == event1.Id));
        }
        [Fact]
        public async Task AddBooking_InvalidEventId_ThrowsDbUpdateException()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var @event = CreateEvent();
            var booking = CreateBooking(@event);
            var context = await _dbWorker.CreateContext();
            var bookingRepository = new BookingsRepository(context);


            //Act
            var result = bookingRepository.Add(booking);

            //Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () => await result);
        }
        [Fact]
        public async Task Confirm_CorrectParameters_ChangeBookingStatusInDataBase()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var context = await _dbWorker.CreateContext();
            var @event = CreateEvent();
            var booking = CreateBooking(@event);
            await context.Events.AddAsync(@event);
            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();


            //Act
            context = await _dbWorker.CreateContext();
            var bookingRepository = new BookingsRepository(context);
            await bookingRepository.Confirm(booking.Id);

            //Assert
            context = await _dbWorker.CreateContext();
            var reqieredBooking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);
            Assert.NotNull(reqieredBooking);
            Assert.Equal(BookingStatus.Confirmed, reqieredBooking.Status);
        }
    }
}
