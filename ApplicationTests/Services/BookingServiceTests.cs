using Application.Repositories;
using Application.Services.BookingService;
using Application.Services.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationTests.Services
{
    public class BookingServiceTests
    {
        private readonly IBookingService _bookingService;
        private readonly Mock<IBookingsRepository> _mockBookingsRepository;
        private readonly Mock<IEventsRepository> _mockEventsRepository;
        private readonly Mock<ILogger<BookingService>> _mockLogger;

        private readonly List<Booking> _bookings;
        private readonly Event _existingEvent;

        public BookingServiceTests()
        {
            _mockLogger = new Mock<ILogger<BookingService>>();
            _mockBookingsRepository = new Mock<IBookingsRepository>();
            _mockEventsRepository = new Mock<IEventsRepository>();

            _existingEvent = CreateTestEvent();

            _bookings =
                [
                    new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2000.01.01"), null, null),
                    new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2001.01.01"), null, null),
                    new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2002.01.01"), null, null)
                ];

            _bookingService = new BookingService(_mockBookingsRepository.Object, _mockEventsRepository.Object);
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
                status ?? EventStatus.Existing,
                totalSeats ?? 4,
                availableSeats ?? 4
            );
        }

        [Fact]
        public async Task CreateBookingAsync_CorrectParam_ReturnBookingInfo()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);
            _mockEventsRepository.Setup(repo => repo.TryReserveSeats(It.IsAny<Guid>())).ReturnsAsync(true);
            _mockBookingsRepository.Setup(repo => repo.Add(It.IsAny<Booking>()));

            //Act
            var result = await _bookingService.CreateBookingAsync(_existingEvent.Id);


            //Assert
            Assert.NotNull(result);
            Assert.Null(result.ProcessedAt);
            Assert.Equal(_existingEvent.Id, result.EventId);
            Assert.Equal(BookingStatus.Pending, result.Status);    
        }

        [Fact]
        public async Task CreateBookingAsync_NotExistingEvent_ReturnNotFoundException()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync((Event?)null);
            _mockEventsRepository.Setup(repo => repo.TryReserveSeats(It.IsAny<Guid>())).ReturnsAsync(true);
            _mockBookingsRepository.Setup(repo => repo.Add(It.IsAny<Booking>()));

            //Act
            var result = _bookingService.CreateBookingAsync(_existingEvent.Id);


            //Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await result);
        }
        [Fact]
        public async Task CreateBookingAsync_EventWithStatusRemoved_ReturnValidationException()
        {
            //Arrange
            _existingEvent.Status = EventStatus.Removed;
            _mockEventsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);
            _mockEventsRepository.Setup(repo => repo.TryReserveSeats(It.IsAny<Guid>())).ReturnsAsync(true);
            _mockBookingsRepository.Setup(repo => repo.Add(It.IsAny<Booking>()));

            //Act
            var result = _bookingService.CreateBookingAsync(_existingEvent.Id);


            //Assert
            await Assert.ThrowsAsync<DomainValidationException>(async () => await result);
        }
        [Fact]
        public async Task CreateBookingAsync_EventWithoutSeats_ReturnNoAvailableSeatsException()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);
            _mockEventsRepository.Setup(repo => repo.TryReserveSeats(It.IsAny<Guid>())).ReturnsAsync(false);
            _mockBookingsRepository.Setup(repo => repo.Add(It.IsAny<Booking>()));

            //Act
            var result = _bookingService.CreateBookingAsync(_existingEvent.Id);


            //Assert
            await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await result);
        }
        [Fact]
        public async Task GetBookingByIdAsync_ExistingId_ReturnBookingInfo()
        {
            //Arrange
            _mockBookingsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_bookings[0]);

            //Act
            var result = await _bookingService.GetBookingByIdAsync(_bookings[0].Id);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(_bookings[0].Id, result.Id);
            Assert.Equal(_bookings[0].EventId, result.EventId);
            Assert.Equal(_bookings[0].Status, result.Status);
            Assert.Equal(_bookings[0].CreatedAt, result.CreatedAt);
            Assert.Equal(_bookings[0].ProcessedAt, result.ProcessedAt);
        }
        [Fact]
        public async Task GetBookingByIdAsync_NotExistingId_ReturnNull()
        {
            //Arrange
            _mockBookingsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync((Booking?)null);

            //Act
            var result = await _bookingService.GetBookingByIdAsync(Guid.NewGuid());

            //Assert
            Assert.Null(result);
        }
    }
}
