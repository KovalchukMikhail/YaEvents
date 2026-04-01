using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using YaEvents.Application.Services.BookingService;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Tests.Application.Services
{
    public class BookingServiceTests
    {
        private readonly BookingService _bookingService;
        private readonly Mock<IRepository<Booking>> _mockBookingRepository;
        private readonly Mock<IRepository<Event>> _mockEventRepository;
        private readonly List<Booking> _bookings;

        public BookingServiceTests()
        {
            _bookings = new List<Booking>();
            _mockBookingRepository = new Mock<IRepository<Booking>>();
            _mockEventRepository = new Mock<IRepository<Event>>();
            _bookingService = new BookingService(_mockBookingRepository.Object, _mockEventRepository.Object);

            _bookings =
                [
                    new Booking{Id = Guid.NewGuid(), EventId = Guid.NewGuid(), CreatedAt = DateTime.Parse("2000.01.01"), Status = BookingStatus.Pending},
                    new Booking{Id = Guid.NewGuid(), EventId = Guid.NewGuid(), CreatedAt = DateTime.Parse("2001.01.01"), Status = BookingStatus.Pending},
                    new Booking{Id = Guid.NewGuid(), EventId = Guid.NewGuid(), CreatedAt = DateTime.Parse("2002.01.01"), Status = BookingStatus.Pending}
                ];
        }

        [Fact]
        public async Task CreateBookingAsync_CorrectParam_CallRepositoryAddMethod()
        {
            //Arrange
            var eventId = Guid.NewGuid();
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));

            //Act
            await _bookingService.CreateBookingAsync(eventId);

            //Assert
            _mockBookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Once);
        }
        [Fact]
        public async Task CreateBookingAsync_CorrectParam_ReturnCorrectBookingInfoWithPendingStatus()
        {
            //Arrange
            var eventId = Guid.NewGuid();
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));

            //Act
            var result = await _bookingService.CreateBookingAsync(eventId);

            //Assert
            Assert.True(result is BookingInfo && result?.Status == BookingStatus.Pending);
        }

        [Fact]
        public async Task CreateBookingAsync_MethodRunSeveralTimesWithOneEventId_ReturnDifferentBookingInfo()
        {
            //Arrange
            var eventId = Guid.NewGuid();
            _mockBookingRepository.Setup(m => m.Add(It.IsAny<Booking>()));

            //Act
            var bookingInfoFirst = _bookingService.CreateBookingAsync(eventId);
            var bookingInfoSecond = _bookingService.CreateBookingAsync(eventId);
            await Task.WhenAll(bookingInfoFirst, bookingInfoSecond);

            //Assert
            Assert.NotEqual((await bookingInfoFirst)?.Id, (await bookingInfoSecond)?.Id);
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
            var existingEvent = new Event { Id = Guid.NewGuid(), Title = "Event001", Description = "Event", StartAt = DateTime.Parse("2000.01.01"), EndAt = DateTime.Parse("2001.01.01"), Status = EventStatus.Existing };
            _mockBookingRepository.Setup(m => m.GetAll()).ReturnsAsync(_bookings);
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);

            //Act
            await _bookingService.ProcessBookings();

            //Assert
            Assert.True(_bookings.All(b => b.Status == BookingStatus.Confirmed));

        }

        [Fact]
        public async Task ProcessBookings_RemovedEvent_BookingsWithStatusRejected()
        {
            //Arrange
            var existingEvent = new Event { Id = Guid.NewGuid(), Title = "Event001", Description = "Event", StartAt = DateTime.Parse("2000.01.01"), EndAt = DateTime.Parse("2001.01.01"), Status = EventStatus.Removed };
            _mockBookingRepository.Setup(m => m.GetAll()).ReturnsAsync(_bookings);
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(existingEvent);

            //Act
            await _bookingService.ProcessBookings();

            //Assert
            Assert.True(_bookings.All(b => b.Status == BookingStatus.Rejected));
        }

        [Fact]
        public async Task ProcessBookings_NotExistingEvent_BookingsWithStatusRejected()
        {
            //Arrange
            _mockBookingRepository.Setup(m => m.GetAll()).ReturnsAsync(_bookings);
            _mockEventRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync((Event?)null);

            //Act
            await _bookingService.ProcessBookings();

            //Assert
            Assert.True(_bookings.All(b => b.Status == BookingStatus.Rejected));
        }
    }
}
