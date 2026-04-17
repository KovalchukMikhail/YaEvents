using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using YaEvents.Application.Services.BookingService;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;
using YaEvents.Infrastructure.Repositories.BookingsRepository;
using YaEvents.Infrastructure.Repositories.Interfaces;
using YaEvents.Presentation.Endpoints;

namespace YaEvents.Tests.Application.Services
{
    public class BookingServiceTests
    {
        private readonly BookingService _bookingService;
        private readonly Mock<BookingsRepository> _mockBookingRepository;
        private readonly Mock<IRepository<Event>> _mockEventRepository;
        private readonly Mock<ILogger<BookingService>> _mockLogger;
        private readonly List<Booking> _bookings;
        private readonly Event _existingEvent;

        public BookingServiceTests()
        {
            _bookings = new List<Booking>();
            _mockBookingRepository = new Mock<BookingsRepository>();
            _mockEventRepository = new Mock<IRepository<Event>>();
            _mockLogger = new Mock<ILogger<BookingService>>();
            _bookingService = new BookingService(_mockBookingRepository.Object, _mockEventRepository.Object, _mockLogger.Object);

            _existingEvent = CreateTestEvent();

            _bookings =
                [
                    new Booking{Id = Guid.NewGuid(), EventId = _existingEvent.Id, CreatedAt = DateTime.Parse("2000.01.01"), Status = BookingStatus.Pending},
                    new Booking{Id = Guid.NewGuid(), EventId = _existingEvent.Id, CreatedAt = DateTime.Parse("2001.01.01"), Status = BookingStatus.Pending},
                    new Booking{Id = Guid.NewGuid(), EventId = _existingEvent.Id, CreatedAt = DateTime.Parse("2002.01.01"), Status = BookingStatus.Pending}
                ];


        }

        public Event CreateTestEvent(string? title = null, string? Description = null, DateTime? startAt = null, DateTime? endAt = null, EventStatus? status = null, int? totalSeats = null, int? availableSeats = null)
        {
            return new Event
            {
                Id = Guid.NewGuid(),
                Title = title ?? "Title",
                Description = Description ?? "Description",
                StartAt = startAt ?? DateTime.Parse("2010.01.01"),
                EndAt = endAt ?? DateTime.Parse("2011.01.01"),
                Status = status ?? Infrastructure.Enums.EventStatus.Existing,
                TotalSeats = totalSeats ?? 3,
                AvailableSeats = availableSeats ?? 3
            };
        }

        [Fact]
        public async Task CreateBookingAsync_CorrectParam_CallBookingRepositoryAddMethod()
        {
            //Arrange
            var existingEvent = CreateTestEvent();
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);

            //Act
            await _bookingService.CreateBookingAsync(existingEvent.Id);

            //Assert
            _mockBookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Once);
        }
        [Fact]
        public async Task CreateBookingAsync_CorrectParam_CallEventRepositoryGetMethod()
        {
            //Arrange
            var existingEvent = CreateTestEvent();
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));

            //Act
            await _bookingService.CreateBookingAsync(existingEvent.Id);

            //Assert
            _mockEventRepository.Verify(repo => repo.Get(It.IsAny<Guid>()), Times.Once);
        }
        [Fact]
        public async Task CreateBookingAsync_CorrectParam_ReturnCorrectBookingInfoWithPendingStatus()
        {
            //Arrange
            var existingEvent = CreateTestEvent();
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));

            //Act
            var result = await _bookingService.CreateBookingAsync(existingEvent.Id);

            //Assert
            Assert.True(result is BookingInfo && result?.Status == BookingStatus.Pending);
        }

        [Fact]
        public async Task CreateBookingAsync_MethodRunSeveralTimesWithOneEventId_ReturnDifferentBookingInfo()
        {
            //Arrange
            var existingEvent = CreateTestEvent();
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));

            //Act
            var bookingInfoFirst = _bookingService.CreateBookingAsync(existingEvent.Id);
            var bookingInfoSecond = _bookingService.CreateBookingAsync(existingEvent.Id);
            await Task.WhenAll(bookingInfoFirst, bookingInfoSecond);

            //Assert
            Assert.NotEqual((await bookingInfoFirst)?.Id, (await bookingInfoSecond)?.Id);
        }
        [Fact]
        public async Task CreateBookingAsync_NotExistingEvent_ThrowNotFoundException()
        {
            //Arrange
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync((Event?)null);

            //Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateBookingAsync_EventWithRemovedStatus_ThrowValidationException()
        {
            //Arrange
            var removedEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.Parse("2010.01.01"),
                EndAt = DateTime.Parse("2011.01.01"),
                Status = Infrastructure.Enums.EventStatus.Removed
            };
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(removedEvent);

            //Act & Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _bookingService.CreateBookingAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateBookingAsync_CorrectParam_CorrectAvailableSeatsAfterBooking()
        {
            //Arrange
            var existingEvent = CreateTestEvent();
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);
            var exceptedAvailableSeats = existingEvent.AvailableSeats - 1;

            //Act
            await _bookingService.CreateBookingAsync(existingEvent.Id);

            //Assert
            Assert.Equal(exceptedAvailableSeats, existingEvent.AvailableSeats);
        }
        [Fact]
        public async Task CreateBookingAsync_BookMoreThanAvailableSeats_ThrowNoAvailableSeatsException()
        {
            //Arrange
            var existingEvent = CreateTestEvent();
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));

            //Act
            var bookingInfoFirst = _bookingService.CreateBookingAsync(existingEvent.Id);
            var bookingInfoSecond = _bookingService.CreateBookingAsync(existingEvent.Id);
            var bookingInfoThird = _bookingService.CreateBookingAsync(existingEvent.Id);
            var bookingInfoForth = _bookingService.CreateBookingAsync(existingEvent.Id);
            var task = Task.WhenAll(bookingInfoFirst, bookingInfoSecond, bookingInfoThird, bookingInfoForth);

            //Assert
            await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await task);
        }
        [Fact]
        public async Task CreateBookingAsync_BookSeatAfterReleaseSeats_CorrectAvailableSeatsCount()
        {
            //Arrange
            var existingEvent = CreateTestEvent();
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));
            var sourceAvailableSeatsVal = existingEvent.AvailableSeats;

            //Act
            var bookingInfoFirst = await _bookingService.CreateBookingAsync(existingEvent.Id);
            var booking = new Booking()
            {   Id = bookingInfoFirst.Id,
                EventId = bookingInfoFirst.EventId,
                CreatedAt = bookingInfoFirst.CreatedAt,
                ProcessedAt = bookingInfoFirst.ProcessedAt,
                Status = bookingInfoFirst.Status };

            var availableSeatsBeforeRejected = existingEvent.AvailableSeats;
            await _bookingService.RejectBookingAsync(booking, existingEvent);
            var availableSeatsAfterRejected = existingEvent.AvailableSeats;
            var bookingInfoSecond = await _bookingService.CreateBookingAsync(existingEvent.Id);
            var availableSeatsAfterCreateNewBooking = existingEvent.AvailableSeats;

            //Assert
            Assert.True(availableSeatsBeforeRejected == sourceAvailableSeatsVal - 1
                        && availableSeatsAfterRejected == sourceAvailableSeatsVal
                        && availableSeatsAfterCreateNewBooking == sourceAvailableSeatsVal - 1);
        }
        [Fact]
        public async Task CreateBookingAsync_FifteenConcurrentRequests_FiveSuccessAndTenException()
        {
            //Arrange
            var totalSeats = 5;
            var availableSeats = totalSeats;
            var existingEvent = CreateTestEvent(totalSeats: totalSeats, availableSeats: availableSeats);
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));

            //Act
            var size = 15;
            var bookings = new Task<BookingInfo>[size];
            Parallel.For(0, size, i => bookings[i] = _bookingService.CreateBookingAsync(existingEvent.Id));

            var tasks = Task.WhenAll(bookings);

            //Assert
            await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await tasks);
            Assert.Equal(availableSeats, bookings.Where(b => b.IsCompletedSuccessfully).Count());
            Assert.Equal(size - availableSeats, bookings.Where(b => b.IsFaulted && b.Exception.InnerException is NoAvailableSeatsException).Count());
            Assert.Equal(0, existingEvent.AvailableSeats);
        }
        [Fact]
        public async Task CreateBookingAsync_TenConcurrentRequests_TenUniqueId()
        {
            //Arrange
            var totalSeats = 10;
            var availableSeats = totalSeats;
            var existingEvent = CreateTestEvent(totalSeats: totalSeats, availableSeats: availableSeats);
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));

            //Act
            var size = 10;
            var bookings = new Task<BookingInfo>[size];
            Parallel.For(0, size, i => bookings[i] = _bookingService.CreateBookingAsync(existingEvent.Id));

            var tasks = Task.WhenAll(bookings);

            //Assert
            Assert.Equal(size, bookings.DistinctBy(b => b.Result.Id).Count());
        }
        [Fact]
        public async Task GetBookingByIdAsync_CorrectParam_CallRepositoryGetMethod()
        {
            //Arrange
            var id = Guid.NewGuid();
            _mockBookingRepository.Setup(m => m.Get(It.IsAny<Guid>()));

            //Act
            await _bookingService.GetBookingByIdAsync(id);

            //Assert
            _mockBookingRepository.Verify(repo => repo.Get(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task GetBookingByIdAsync_CorrectId_ReturnBookingInfo()
        {
            //Arrange
            var booking = _bookings[0];
            _mockBookingRepository.Setup(m => m.Get(booking.Id)).ReturnsAsync(booking);

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
            _mockBookingRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(_bookings.FirstOrDefault(b => b.Id == id));

            //Act
            var result = await _bookingService.GetBookingByIdAsync(id);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ProcessBookings_BookingsWithSourceStatusPending_BookingsWithStatusConfirmed()
        {
            //Arrange
            _mockBookingRepository.Setup(m => m.GetPending()).ReturnsAsync(_bookings);
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);

            //Act
            await _bookingService.ProcessBookings();

            //Assert
            Assert.True(_bookings.All(b => b.Status == BookingStatus.Confirmed));

        }

        [Fact]
        public async Task ProcessBookingAsync_CorrectParameters_BookingStatusEqualConfirmed()
        {
            //Arrange
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);

            //Act
            await _bookingService.ProcessBookingAsync(_bookings[0]);

            //Assert
            Assert.Equal(BookingStatus.Confirmed, _bookings[0].Status);

        }
        [Fact]
        public async Task ProcessBookingAsync_CorrectParameters_ProcessedAtNotNull()
        {
            //Arrange
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);

            //Act
            await _bookingService.ProcessBookingAsync(_bookings[0]);

            //Assert
            Assert.NotNull(_bookings[0].ProcessedAt);
        }
        [Fact]
        public async Task ProcessBookingAsync_EventStatusRemoved_ThrowValidationException()
        {
            //Arrange
            _existingEvent.Status = EventStatus.Removed;
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);

            //Act
            var task = _bookingService.ProcessBookingAsync(_bookings[0]);

            //Act & Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await task);
        }
        [Fact]
        public async Task ProcessBookingAsync_EventIsNull_ThrowValidationException()
        {
            //Arrange
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync((Event?)null);

            //Act
            var task = _bookingService.ProcessBookingAsync(_bookings[0]);

            //Act & Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await task);
        }

        [Fact]
        public async Task RejectBookingAsync_CorrectParameters_BookingStatusEqualRejected()
        {
            //Arrange
            _mockEventRepository.Setup(m => m.Update(It.IsAny<Event>()));
            _mockBookingRepository.Setup(m => m.Update(It.IsAny<Booking>()));

            //Act
            await _bookingService.RejectBookingAsync(_bookings[0], _existingEvent);

            //Assert
            Assert.Equal(BookingStatus.Rejected, _bookings[0].Status);
        }
        [Fact]
        public async Task RejectBookingAsync_CorrectParameters_ProcessedAtChanged()
        {
            //Arrange
            _mockEventRepository.Setup(m => m.Update(It.IsAny<Event>()));
            _mockBookingRepository.Setup(m => m.Update(It.IsAny<Booking>()));
            var processedAt = _bookings[0].ProcessedAt;

            //Act
            await _bookingService.RejectBookingAsync(_bookings[0], _existingEvent);

            //Assert
            Assert.NotEqual(processedAt, _bookings[0].ProcessedAt);
        }
        [Fact]
        public async Task RejectBookingAsync_CorrectParameters_CorrectAvailableSeats()
        {
            //Arrange
            var exceptedSeats = 3;
            _existingEvent.TryReserveSeats();
            _mockEventRepository.Setup(m => m.Update(It.IsAny<Event>()));
            _mockBookingRepository.Setup(m => m.Update(It.IsAny<Booking>()));

            //Act
            await _bookingService.RejectBookingAsync(_bookings[0], _existingEvent);

            //Assert
            Assert.Equal(exceptedSeats, _existingEvent.AvailableSeats);
        }

    }
}
