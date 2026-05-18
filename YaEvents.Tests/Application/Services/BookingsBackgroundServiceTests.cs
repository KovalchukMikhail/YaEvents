using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using YaEvents.Application.BackgroundServices;
using YaEvents.Application.Services.BookingService;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;

namespace YaEvents.Tests.Application.Services
{
    public class BookingsBackgroundServiceTests
    {
        private readonly BookingsBackgroundService _bookingsBackgroundService;
        private readonly Mock<ILogger<BookingsBackgroundService>> _mockLogger;
        private readonly List<Booking> _bookings;
        private readonly Event _existingEvent;
        private readonly Event _removedEvent;

        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IServiceScopeFactory _scopeFactory;

        public BookingsBackgroundServiceTests()
        {
            _mockLogger = new Mock<ILogger<BookingsBackgroundService>>();
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
            services.AddScoped<BookingsBackgroundService>();
            services.AddSingleton(_mockLogger.Object);

            _existingEvent = CreateTestEvent();
            _removedEvent = CreateTestEvent(status: EventStatus.Removed);
            _bookings =
                [
                    new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2000.01.01"), null, _existingEvent),
                                new Booking(Guid.NewGuid(), _removedEvent.Id, BookingStatus.Pending, DateTime.Parse("2001.01.01"), null, _removedEvent),
                                new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2002.01.01"), null, _existingEvent)
                ];

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var appDbContext = _scope.ServiceProvider.GetService<AppDbContext>();
            appDbContext.Events.Add(_removedEvent);
            appDbContext.Events.Add(_existingEvent);
            appDbContext.Bookings.AddRange(_bookings);
            appDbContext.SaveChanges();

            _bookingsBackgroundService = _scope.ServiceProvider.GetRequiredService<BookingsBackgroundService>();
        }

        public Event CreateTestEvent(string? title = null, string? Description = null, DateTime? startAt = null, DateTime? endAt = null, EventStatus? status = null, int? totalSeats = null, int? availableSeats = null)
        {
            return new Event
            (
                Guid.NewGuid(),
                title ?? "Title",
                Description ?? "Description",
                startAt ?? DateTime.Parse("2010.01.01"),
                endAt ?? DateTime.Parse("2011.01.01"),
                status ?? Infrastructure.Enums.EventStatus.Existing,
                totalSeats ?? 4,
                availableSeats ?? 4
            );
        }


        [Fact]
        public async Task ProcessBookings_BookingsWithSourceStatusPending_BookingsWithStatusConfirmed()
        {
            //Arrange
            var bookingId = _bookings[0].Id;

            //Act
            await _bookingsBackgroundService.ProcessBookingAsync(bookingId);

            //Assert
            var scope = _scopeFactory.CreateScope();
            var appDbContext = scope.ServiceProvider.GetService<AppDbContext>();
            var booking = appDbContext?.Bookings.FirstOrDefault(b => b.Id == bookingId);
            Assert.True(booking != null && booking.Status == BookingStatus.Confirmed);
        }

        [Fact]
        public async Task ProcessBookingAsync_CorrectParameters_ProcessedAtNotNull()
        {
            //Arrange
            var bookingId = _bookings[0].Id;

            //Act
            await _bookingsBackgroundService.ProcessBookingAsync(bookingId);

            //Assert
            var scope = _scopeFactory.CreateScope();
            var appDbContext = scope.ServiceProvider.GetService<AppDbContext>();
            var booking = appDbContext?.Bookings.FirstOrDefault(b => b.Id == bookingId);
            Assert.NotNull(booking?.ProcessedAt);
        }
        [Fact]
        public async Task ProcessBookingAsync_EventStatusRemoved_BookingStatusReject()
        {
            //Arrange
            var bookingId = _bookings[1].Id;

            //Act
            await _bookingsBackgroundService.ProcessBookingAsync(bookingId);

            //Assert
            var scope = _scopeFactory.CreateScope();
            var appDbContext = scope.ServiceProvider.GetService<AppDbContext>();
            var booking = appDbContext?.Bookings.FirstOrDefault(b => b.Id == bookingId);
            Assert.Equal(BookingStatus.Rejected, booking.Status);
        }

        [Fact]
        public async Task RejectBookingAsync_CorrectParameters_BookingStatusReject()
        {
            //Arrange
            var bookingId = _bookings[0].Id;
            var scope = _scopeFactory.CreateScope();
            var appDbContext = scope.ServiceProvider.GetService<AppDbContext>();
            var booking = appDbContext?.Bookings.FirstOrDefault(b => b.Id == bookingId);
            var curEvent = appDbContext?.Events.FirstOrDefault(e => e.Id == booking.EventId);

            //Act
            await _bookingsBackgroundService.RejectBookingAsync(booking, curEvent, appDbContext);

            //Assert
            Assert.Equal(BookingStatus.Rejected, booking.Status);
        }
        [Fact]
        public async Task RejectBookingAsync_CorrectParameters_ProcessedAtNotNull()
        {
            //Arrange
            var bookingId = _bookings[0].Id;
            var scope = _scopeFactory.CreateScope();
            var appDbContext = scope.ServiceProvider.GetService<AppDbContext>();
            var booking = appDbContext?.Bookings.FirstOrDefault(b => b.Id == bookingId);
            var curEvent = appDbContext?.Events.FirstOrDefault(e => e.Id == booking.EventId);

            //Act
            await _bookingsBackgroundService.RejectBookingAsync(booking, curEvent, appDbContext);

            //Assert
            Assert.NotNull(booking.ProcessedAt);
        }
        [Fact]
        public async Task RejectBookingAsync_CorrectParameters_CorrectAvailableSeats()
        {
            //Arrange
            var bookingId = _bookings[0].Id;
            var scope = _scopeFactory.CreateScope();
            var appDbContext = scope.ServiceProvider.GetService<AppDbContext>();
            var booking = appDbContext?.Bookings.FirstOrDefault(b => b.Id == bookingId);
            var curEvent = appDbContext?.Events.FirstOrDefault(e => e.Id == booking.EventId);
            curEvent.TryReserveSeats();

            //Act
            await _bookingsBackgroundService.RejectBookingAsync(booking, curEvent, appDbContext);

            //Assert
            Assert.Equal(4, curEvent.AvailableSeats);
        }
    }
}
