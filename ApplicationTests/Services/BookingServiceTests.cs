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
        private readonly Mock<IUsersRepository> _mockUsersRepository;
        private readonly Mock<ILogger<BookingService>> _mockLogger;

        private readonly List<Booking> _bookings;
        private readonly Event _existingEvent;
        private readonly User _existingUser;
        private readonly int _limitOfBookings = 10;

        public BookingServiceTests()
        {
            _mockLogger = new Mock<ILogger<BookingService>>();
            _mockBookingsRepository = new Mock<IBookingsRepository>();
            _mockEventsRepository = new Mock<IEventsRepository>();
            _mockUsersRepository = new Mock<IUsersRepository>();

            _existingEvent = CreateTestEvent();
            _existingUser = CreateTestUser();

            _bookings =
                [
                    new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2000.01.01"), processedAt: null, null, _existingUser.Id, null),
                    new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2001.01.01"), processedAt: null, null, _existingUser.Id, null),
                    new Booking(Guid.NewGuid(), _existingEvent.Id, BookingStatus.Pending, DateTime.Parse("2002.01.01"), processedAt: null, null, _existingUser.Id, null)
                ];

            _bookingService = new BookingService(_mockBookingsRepository.Object, _mockEventsRepository.Object, _mockUsersRepository.Object);
        }

        private Event CreateTestEvent(string? title = null, string? Description = null, DateTime? startAt = null, DateTime? endAt = null, EventStatus? status = null, int? totalSeats = null, int? availableSeats = null)
        {
            return new Event
            (
                Guid.NewGuid(),
                title ?? "Title",
                Description ?? "Description",
                startAt ?? DateTime.Parse("2026.08.01"),
                endAt ?? DateTime.Parse("2026.09.01"),
                status ?? EventStatus.Existing,
                totalSeats ?? 4,
                availableSeats ?? 4
            );
        }
        private User CreateTestUser(string? login = null, string? passwordHash = null, UserRole? role = null)
        {
            return new User
                (
                    Guid.NewGuid(),
                    login ?? "Test",
                    passwordHash ?? "Test".GetHashCode().ToString(),
                    role ?? UserRole.User
                );
        }
        private Booking CreateTestBooking(Event @event = null,
                                            BookingStatus? bookingStatus = null,
                                            DateTime? createdAt = null,
                                            DateTime? processedAt = null,
                                            bool addEvent = false,
                                            User user = null,
                                            bool addUser = false)
        {
            return new Booking(Guid.NewGuid(),
                        @event?.Id ?? _existingEvent.Id,
                        bookingStatus ?? BookingStatus.Pending,
                        createdAt ?? DateTime.Parse("2000.01.01"),
                        processedAt ?? null,
                        addEvent ? @event : null,
                        user?.Id ?? _existingUser.Id,
                        addUser ? user : null
                    );
        }

        [Fact]
        public async Task CreateBookingAsync_CorrectParam_ReturnBookingInfo()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);
            _mockEventsRepository.Setup(repo => repo.TryReserveSeats(It.IsAny<Guid>())).ReturnsAsync(true);
            _mockBookingsRepository.Setup(repo => repo.Add(It.IsAny<Booking>()));
            _mockUsersRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingUser);

            //Act
            var result = await _bookingService.CreateBookingAsync(_existingEvent.Id, _existingUser.Id, _limitOfBookings);


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
            _mockUsersRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingUser);

            //Act
            var result = _bookingService.CreateBookingAsync(_existingEvent.Id, _existingUser.Id, _limitOfBookings);


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
            _mockUsersRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingUser);

            //Act
            var result = _bookingService.CreateBookingAsync(_existingEvent.Id, _existingUser.Id, _limitOfBookings);


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
            _mockUsersRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingUser);

            //Act
            var result = _bookingService.CreateBookingAsync(_existingEvent.Id, _existingUser.Id, _limitOfBookings);


            //Assert
            await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await result);
        }
        [Fact]
        public async Task CreateBookingAsync_PastEvent_ThrowBookingPastEventException()
        {
            //Arrange
            _existingEvent.StartAt = DateTime.Today.ToUniversalTime();
            _mockEventsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);
            _mockEventsRepository.Setup(repo => repo.TryReserveSeats(It.IsAny<Guid>())).ReturnsAsync(false);
            _mockBookingsRepository.Setup(repo => repo.Add(It.IsAny<Booking>()));
            _mockUsersRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingUser);

            //Act
            var result = _bookingService.CreateBookingAsync(_existingEvent.Id, _existingUser.Id, _limitOfBookings);


            //Assert
            await Assert.ThrowsAsync<BookingPastEventException>(async () => await result);
        }
        [Fact]
        public async Task CreateBookingAsync_BookingLimitExceeded_ThrowLimitOfActiveBookingsExceededException()
        {
            //Arrange
            _existingUser.Bookings = new List<Booking>(_bookings);
            _existingUser.Bookings.ForEach(b => b.Event = _existingEvent);
            var limitOfBookings = 3;
            _mockEventsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);
            _mockEventsRepository.Setup(repo => repo.TryReserveSeats(It.IsAny<Guid>())).ReturnsAsync(false);
            _mockBookingsRepository.Setup(repo => repo.Add(It.IsAny<Booking>()));
            _mockUsersRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingUser);

            //Act
            var result = _bookingService.CreateBookingAsync(_existingEvent.Id, _existingUser.Id, limitOfBookings);


            //Assert
            await Assert.ThrowsAsync<LimitOfActiveBookingsExceededException>(async () => await result);
        }
        [Fact]
        public async Task CreateBookingAsync_BookingLimitOfSomeUsersDoesNotAffectOthers_ThrowLimitOfActiveBookingsExceededExceptionForOneAndCorectResultForOthers()
        {
            //Arrange
            var user1 = CreateTestUser("User1");
            var user2 = CreateTestUser("User2");
            user1.Bookings = new List<Booking>(_bookings);
            user1.Bookings.ForEach(b => b.Event = _existingEvent);
            user2.Bookings = new List<Booking>();
            var limitOfBookings = 3;
            _mockEventsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingEvent);
            _mockEventsRepository.Setup(repo => repo.TryReserveSeats(It.IsAny<Guid>())).ReturnsAsync(true);
            _mockBookingsRepository.Setup(repo => repo.Add(It.IsAny<Booking>()));
            _mockUsersRepository.Setup(repo => repo.Get(user1.Id)).ReturnsAsync(user1);
            _mockUsersRepository.Setup(repo => repo.Get(user2.Id)).ReturnsAsync(user2);

            //Act
            var user1ResultTask = _bookingService.CreateBookingAsync(_existingEvent.Id, user1.Id, limitOfBookings);
            var user2ResultTask = _bookingService.CreateBookingAsync(_existingEvent.Id, user2.Id, limitOfBookings);


            //Assert
            await Assert.ThrowsAsync<LimitOfActiveBookingsExceededException>(async () => await user1ResultTask);
            var user2Result = await user2ResultTask;
            Assert.NotNull(user2Result);
            Assert.Null(user2Result.ProcessedAt);
            Assert.Equal(_existingEvent.Id, user2Result.EventId);
            Assert.Equal(BookingStatus.Pending, user2Result.Status);
        }
        [Fact]
        public async Task GetBooking_ExistingId_ReturnBookingInfo()
        {
            //Arrange
            _bookings[0].User = _existingUser;
            _mockBookingsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_bookings[0]);
            _mockUsersRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingUser);

            //Act
            var result = await _bookingService.GetBooking(_bookings[0].Id, _existingUser.Id);
        
            //Assert
            Assert.NotNull(result);
            Assert.Equal(_bookings[0].Id, result.Id);
            Assert.Equal(_bookings[0].EventId, result.EventId);
            Assert.Equal(_bookings[0].Status, result.Status);
            Assert.Equal(_bookings[0].CreatedAt, result.CreatedAt);
            Assert.Equal(_bookings[0].ProcessedAt, result.ProcessedAt);
        }
        [Fact]
        public async Task GetBooking_NotExistingId_ThrowNotFoundException()
        {
            //Arrange
            _mockBookingsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync((Booking?)null);
            _mockUsersRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(_existingUser);

            //Act
            var result = _bookingService.GetBooking(Guid.NewGuid(), _existingUser.Id);

            //Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await result);
        }
        [Fact]
        public async Task CancelBooking_UserCancelHisBooking_ReturnsTrue()
        {
            //Arrange
            var user = CreateTestUser("user1", role: UserRole.User);
            var booking = CreateTestBooking(_existingEvent, BookingStatus.Confirmed, DateTime.Parse("2026.06.01"), DateTime.Parse("2026.06.01"), true, user, true);
            _mockUsersRepository.Setup(repo => repo.Get(user.Id)).ReturnsAsync(user);
            _mockBookingsRepository.Setup(repo => repo.Get(booking.Id)).ReturnsAsync(booking);
            _mockBookingsRepository.Setup(repo => repo.Cancel(booking.Id)).ReturnsAsync(true);

            //Act
            var result = await _bookingService.CancelBooking(user.Id, booking.Id);


            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task CancelBooking_UserCancelNotHisBooking_ThrowNoRightsToOperationException()
        {
            //Arrange
            var user1 = CreateTestUser("user1", role: UserRole.User);
            var user2 = CreateTestUser("user2", role: UserRole.User);
            var booking = CreateTestBooking(_existingEvent, BookingStatus.Confirmed, DateTime.Parse("2026.06.01"), DateTime.Parse("2026.06.01"), true, user1, true);
            _mockUsersRepository.Setup(repo => repo.Get(user2.Id)).ReturnsAsync(user2);
            _mockBookingsRepository.Setup(repo => repo.Get(booking.Id)).ReturnsAsync(booking);
            _mockBookingsRepository.Setup(repo => repo.Cancel(booking.Id)).ReturnsAsync(true);


            //Act
            var result = _bookingService.CancelBooking(user2.Id, booking.Id);

            //Assert
            await Assert.ThrowsAsync<NoRightsToOperationException>(async () => await result);
        }

        [Fact]
        public async Task CancelBooking_AdminCancelBooking_ReturnsTrue()
        {
            //Arrange
            var user1 = CreateTestUser("user1", role: UserRole.User);
            var admin = CreateTestUser("admin", role: UserRole.Admin);
            var booking = CreateTestBooking(_existingEvent, BookingStatus.Confirmed, DateTime.Parse("2026.06.01"), DateTime.Parse("2026.06.01"), true, user1, true);
            _mockUsersRepository.Setup(repo => repo.Get(admin.Id)).ReturnsAsync(admin);
            _mockBookingsRepository.Setup(repo => repo.Get(booking.Id)).ReturnsAsync(booking);
            _mockBookingsRepository.Setup(repo => repo.Cancel(booking.Id)).ReturnsAsync(true);

            //Act
            var result = await _bookingService.CancelBooking(admin.Id, booking.Id);

            //Assert
            Assert.True(result);
        }
    }
}
