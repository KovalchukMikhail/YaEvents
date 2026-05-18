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
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;
using YaEvents.Infrastructure.Repositories.BookingsRepository;
using YaEvents.Infrastructure.Repositories.Interfaces;
using YaEvents.Presentation.Endpoints;

namespace YaEvents.Tests.Application.Services
{
    public class BookingServiceTests
    {
        private readonly IBookingService _bookingService;
        private readonly BookingsBackgroundService _bookingsBackgroundService;
        //private readonly Mock<BookingsRepository> _mockBookingRepository;
        //private readonly Mock<IRepository<Event>> _mockEventRepository;
        private readonly Mock<ILogger<BookingService>> _mockLogger;
        private readonly Mock<ILogger<BookingsBackgroundService>> _mockBookingsBackgroundServiceLogger;
        private readonly List<Booking> _bookings;
        private readonly Event _existingEvent;
        private readonly Event _removedEvent;
        private readonly Event _eventWithFiveTotalSeats;
        private readonly Event _eventWithTenTotalSeats;
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IServiceScopeFactory _scopeFactory;

        public BookingServiceTests()
        {
            _mockLogger = new Mock<ILogger<BookingService>>();
            _mockBookingsBackgroundServiceLogger = new Mock<ILogger<BookingsBackgroundService>>();

            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<BookingsBackgroundService>();
            services.AddSingleton(_mockLogger.Object);
            services.AddSingleton(_mockBookingsBackgroundServiceLogger.Object);

            _existingEvent = CreateTestEvent();
            _removedEvent = CreateTestEvent(status: EventStatus.Removed);
            _eventWithFiveTotalSeats = CreateTestEvent(totalSeats: 5, availableSeats: 5);
            _eventWithTenTotalSeats = CreateTestEvent(totalSeats: 10, availableSeats: 10);

            _bookings =
                [
                    new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2000.01.01"), null, null),
                    new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2001.01.01"), null, null),
                    new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2002.01.01"), null, null)
                ];

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var appDbContext = _scope.ServiceProvider.GetService<AppDbContext>();
            appDbContext.Bookings.AddRange(_bookings);
            appDbContext.Events.Add(_existingEvent);
            appDbContext.Events.Add(_removedEvent);
            appDbContext.Events.Add(_eventWithFiveTotalSeats);
            appDbContext.Events.Add(_eventWithTenTotalSeats);
            appDbContext.SaveChanges();

            _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
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
        public async Task CreateBookingAsync_CorrectParam_ReturnCorrectBookingInfoWithPendingStatus()
        {
            //Arrange
            var existingEvent = _existingEvent;

            //Act
            var result = await _bookingService.CreateBookingAsync(existingEvent.Id);

            //Assert
            Assert.True(result is BookingInfo && result?.Status == BookingStatus.Pending);
        }

        [Fact]
        public async Task CreateBookingAsync_MethodRunSeveralTimesWithOneEventId_ReturnDifferentBookingInfo()
        {
            //Arrange

            //Act
            var bookingInfoFirst = _bookingService.CreateBookingAsync(_existingEvent.Id);
            var bookingInfoSecond = _bookingService.CreateBookingAsync(_existingEvent.Id);
            await Task.WhenAll(bookingInfoFirst, bookingInfoSecond);

            //Assert
            Assert.NotEqual((await bookingInfoFirst)?.Id, (await bookingInfoSecond)?.Id);
        }
        [Fact]
        public async Task CreateBookingAsync_NotExistingEvent_ThrowNotFoundException()
        {
            //Arrange

            //Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateBookingAsync_EventWithRemovedStatus_ThrowValidationException()
        {
            //Arrange
            var removedEvent = new Event
            (
                Guid.NewGuid(),
                "Title",
                "Description",
                DateTime.Parse("2010.01.01"),
                DateTime.Parse("2011.01.01"),
                Infrastructure.Enums.EventStatus.Removed,
                3,
                3
            );

            //Act & Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _bookingService.CreateBookingAsync(_removedEvent.Id));
        }

        [Fact]
        public async Task CreateBookingAsync_CorrectParam_CorrectAvailableSeatsAfterBooking()
        {
            //Arrange
            var exceptedAvailableSeats = _existingEvent.AvailableSeats - 1;

            //Act
            await _bookingService.CreateBookingAsync(_existingEvent.Id);

            //Assert
            Assert.Equal(exceptedAvailableSeats, _existingEvent.AvailableSeats);
        }
        [Fact]
        public async Task CreateBookingAsync_BookMoreThanAvailableSeats_ThrowNoAvailableSeatsException()
        {
            //Arrange

            //Act
            var bookingInfoFirst = _bookingService.CreateBookingAsync(_existingEvent.Id);
            var bookingInfoSecond = _bookingService.CreateBookingAsync(_existingEvent.Id);
            var bookingInfoThird = _bookingService.CreateBookingAsync(_existingEvent.Id);
            var bookingInfoForth = _bookingService.CreateBookingAsync(_existingEvent.Id);
            var bookingInfoFifth = _bookingService.CreateBookingAsync(_existingEvent.Id);
            var task = Task.WhenAll(bookingInfoFirst, bookingInfoSecond, bookingInfoThird, bookingInfoForth, bookingInfoFifth);

            //Assert
            await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await task);
        }
        [Fact]
        public async Task CreateBookingAsync_BookSeatAfterReleaseSeats_CorrectAvailableSeatsCount()
        {
            //Arrange
            var sourceAvailableSeatsVal = _existingEvent.AvailableSeats;
            var scope = _scopeFactory.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var bookingsBackgroundService = scope.ServiceProvider.GetRequiredService<BookingsBackgroundService>();
            var appDbContext = scope.ServiceProvider.GetService<AppDbContext>();

            //Act
            var bookingInfoFirst = await bookingService.CreateBookingAsync(_existingEvent.Id);
            var booking = appDbContext?.Bookings.FirstOrDefault(b => b.Id == bookingInfoFirst.Id);
            var curEvent = appDbContext?.Events.FirstOrDefault(e => e.Id == booking.EventId);

            var availableSeatsBeforeRejected = curEvent.AvailableSeats;

            await bookingsBackgroundService.RejectBookingAsync(booking, curEvent, appDbContext);
            var availableSeatsAfterRejected = curEvent.AvailableSeats;
            var bookingInfoSecond = await bookingService.CreateBookingAsync(curEvent.Id);
            var availableSeatsAfterCreateNewBooking = curEvent.AvailableSeats;

            //Assert
            Assert.True(availableSeatsBeforeRejected == sourceAvailableSeatsVal - 1
                        && availableSeatsAfterRejected == sourceAvailableSeatsVal
                        && availableSeatsAfterCreateNewBooking == sourceAvailableSeatsVal - 1);
        }
        [Fact]
        public async Task CreateBookingAsync_FifteenConcurrentRequests_FiveSuccessAndTenException()
        {
            //Arrange
            var totalSeats = _eventWithFiveTotalSeats.TotalSeats;
            var availableSeats = totalSeats;    
                
            //Act
            var size = 15;
            var successBookings = 0;
            var errorBookings = 0;

            var tasks = Enumerable.Range(0, size)
                .Select(_ =>
                {
                    return Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                            await bookingService.CreateBookingAsync(_eventWithFiveTotalSeats.Id);
                            successBookings++;
                        }
                        catch (NoAvailableSeatsException)
                        {
                            errorBookings++;
                        }
                    });
                });

            await Task.WhenAll(tasks);

            //Assert
            Assert.Equal(totalSeats, successBookings);
            Assert.Equal(size - totalSeats, errorBookings);
        }
        [Fact]
        public async Task CreateBookingAsync_TenConcurrentRequests_TenUniqueId()
        {
            //Arrange
            var totalSeats = _eventWithTenTotalSeats.TotalSeats;
            var availableSeats = totalSeats;

            //Act
            var size = 10;
            var ids = new List<Guid>();
            var tasks = Enumerable.Range(0, size)
                .Select(_ =>
                {
                    return Task.Run(async () =>
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                        var booking = await bookingService.CreateBookingAsync(_eventWithTenTotalSeats.Id);
                        ids.Add(booking.Id);

                    });
                });

            await Task.WhenAll(tasks);

            //Assert
            Assert.Equal(size, ids.Distinct().Count());
        }

        [Fact]
        public async Task GetBookingByIdAsync_CorrectId_ReturnBookingInfo()
        {
            //Arrange
            var booking = _bookings[0];

            //Act
            var result = await _bookingService.GetBookingByIdAsync(booking.Id);

            //Assert
            Assert.True(result is BookingInfo
                     && result?.Id == booking.Id
                     && result?.EventId == booking.EventId
                     && result?.CreatedAt == booking.CreatedAt
                     && result?.ProcessedAt == booking.ProcessedAt
                     && result?.Status == booking.Status
                     );
        }

        [Fact]
        public async Task GetBookingByIdAsync_NotCorrectId_ReturnNull()
        {
            //Arrange
            var id = Guid.NewGuid();

            //Act
            var result = await _bookingService.GetBookingByIdAsync(id);

            //Assert
            Assert.Null(result);
        }

    }
}
