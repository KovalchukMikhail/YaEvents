using Application.Cache;
using Application.DTO;
using Application.Repositories;
using Application.Services.EventService;
using Application.Services.Interfaces;
using Domain.Enums;
using Domain.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsTests.ApplicationTests.Services
{
    public class EventServiceTests
    {
        private readonly IEventService _eventService;
        private readonly Mock<IEventsRepository> _mockEventsRepository;
        private readonly Mock<ICasheRepository> _mockCasheRepository;
        private readonly List<Event> _events;

        public EventServiceTests()
        {
            _events =
            [
                new Event(Guid.NewGuid(), "Event001", "Event", DateTime.Parse("2000.01.01"), DateTime.Parse("2001.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event002", "Event", DateTime.Parse("2001.01.01"), DateTime.Parse("2002.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event003", "Event", DateTime.Parse("2002.01.01"), DateTime.Parse("2003.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event004", "Event", DateTime.Parse("2003.01.01"), DateTime.Parse("2004.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event005", "Event", DateTime.Parse("2004.01.01"), DateTime.Parse("2005.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event006", "Event", DateTime.Parse("2005.01.01"), DateTime.Parse("2006.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event007", "Event", DateTime.Parse("2006.01.01"), DateTime.Parse("2007.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event008", "Event", DateTime.Parse("2007.01.01"), DateTime.Parse("2008.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event009", "Event", DateTime.Parse("2008.01.01"), DateTime.Parse("2009.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event010", "Event", DateTime.Parse("2009.01.01"), DateTime.Parse("2010.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event011", "Event", DateTime.Parse("2010.01.01"), DateTime.Parse("2011.01.01"), EventStatus.Existing, 100, 100),
                new Event(Guid.NewGuid(), "Event012", "Event", DateTime.Parse("2011.01.01"), DateTime.Parse("2012.01.01"), EventStatus.Existing, 100, 100)
            ];

            _mockEventsRepository = new Mock<IEventsRepository>();
            _mockCasheRepository = new Mock<ICasheRepository>();
            _eventService = new EventService(_mockEventsRepository.Object, _mockCasheRepository.Object);
        }

        [Fact]
        public async Task GetEvent_EmptyCashe_ReturnEventInfoWithRepository()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Get(_events[0].Id)).ReturnsAsync(_events[0]);
            _mockCasheRepository.Setup(repo => repo.GetEvent(_events[0].Id)).ReturnsAsync((Event?)null);
            var expectedEventInfo = new EventInfo(_events[0].Id, _events[0].Title, _events[0].Description, _events[0].StartAt, _events[0].EndAt, _events[0].Status, _events[0].TotalSeats, _events[0].AvailableSeats);

            //Act
            var result = await _eventService.GetEvent(_events[0].Id);

            //Assert
            _mockCasheRepository.Verify(repo => repo.GetEvent(_events[0].Id), Times.Once);
            _mockEventsRepository.Verify(repo => repo.Get(_events[0].Id), Times.Once);
            _mockCasheRepository.Verify(repo => repo.AddEventToCashe(_events[0]), Times.Once);
            Assert.True(result != null
                    && result.Id == expectedEventInfo.Id
                    && result.AvailableSeats == expectedEventInfo.AvailableSeats
                    && result.Description == expectedEventInfo.Description
                    && result.Title == expectedEventInfo.Title
                    && result.TotalSeats == expectedEventInfo.TotalSeats
                    && result.StartAt == expectedEventInfo.StartAt
                    && result.EndAt == expectedEventInfo.EndAt
                    && result.Status == expectedEventInfo.Status
                    );
        }
        [Fact]
        public async Task GetEvent_NotEmptyCashe_ReturnEventInfoWithCashe()
        {
            //Arrange
            _mockCasheRepository.Setup(repo => repo.GetEvent(_events[0].Id)).ReturnsAsync(_events[0]);
            var expectedEventInfo = new EventInfo(_events[0].Id, _events[0].Title, _events[0].Description, _events[0].StartAt, _events[0].EndAt, _events[0].Status, _events[0].TotalSeats, _events[0].AvailableSeats);

            //Act
            var result = await _eventService.GetEvent(_events[0].Id);

            //Assert
            _mockCasheRepository.Verify(repo => repo.GetEvent(_events[0].Id), Times.Once);
            _mockEventsRepository.Verify(repo => repo.Get(_events[0].Id), Times.Never);
            Assert.True(result != null
                    && result.Id == expectedEventInfo.Id
                    && result.AvailableSeats == expectedEventInfo.AvailableSeats
                    && result.Description == expectedEventInfo.Description
                    && result.Title == expectedEventInfo.Title
                    && result.TotalSeats == expectedEventInfo.TotalSeats
                    && result.StartAt == expectedEventInfo.StartAt
                    && result.EndAt == expectedEventInfo.EndAt
                    && result.Status == expectedEventInfo.Status
                    );
        }
        [Fact]
        public async Task GetEvent_NotCorrectId_ReturnNull()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Get(_events[0].Id)).ReturnsAsync((Event?)null);
            _mockCasheRepository.Setup(repo => repo.GetEvent(_events[0].Id)).ReturnsAsync((Event?)null);

            //Act
            var result = await _eventService.GetEvent(_events[0].Id);

            //Assert
            _mockCasheRepository.Verify(repo => repo.GetEvent(_events[0].Id), Times.Once);
            _mockEventsRepository.Verify(repo => repo.Get(_events[0].Id), Times.Once);
            Assert.Null(result);
        }
        [Fact]
        public async Task PutEvent_CorrectParameters_CallRemoveEventFromCacheMethod()
        {
            //Arrange
            var createEvent = new CreateEvent()
            {
                Description = _events[0].Description!,
                Title = _events[0].Title!,
                TotalSeats = _events[0].TotalSeats,
                StartAt = _events[0].StartAt,
                EndAt = _events[0].EndAt
            };
            _mockEventsRepository.Setup(repo => repo.Update(_events[0].Id, createEvent));


            //Act
            await _eventService.PutEvent(_events[0].Id, createEvent);


            //Assert
            _mockCasheRepository.Verify(repo => repo.RemoveEventFromCache(_events[0].Id), Times.Once);
        }
        [Fact]
        public async Task PutEvent_CorrectParameters_ReturnTrue()
        {
            //Arrange
            var createEvent = new CreateEvent()
            {
                Description = _events[0].Description!,
                Title = _events[0].Title!,
                TotalSeats = _events[0].TotalSeats,
                StartAt = _events[0].StartAt,
                EndAt = _events[0].EndAt
            };
            _mockEventsRepository.Setup(repo => repo.Update(_events[0].Id, createEvent)).ReturnsAsync(true);


            //Act
            var result = await _eventService.PutEvent(_events[0].Id, createEvent);

            //Assert
            _mockEventsRepository.Verify(repo => repo.Update(_events[0].Id, createEvent), Times.Once);
            Assert.True(result);
        }
        [Fact]
        public async Task DeleteEvent_CorrectParameters_CallRemoveEventFromCacheMethod()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Delete(_events[0].Id));


            //Act
            await _eventService.DeleteEvent(_events[0].Id);


            //Assert
            _mockCasheRepository.Verify(repo => repo.RemoveEventFromCache(_events[0].Id), Times.Once);
        }
        [Fact]
        public async Task DeleteEvent_CorrectParameters_ReturnTrue()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Delete(_events[0].Id)).ReturnsAsync(true);


            //Act
            var result = await _eventService.DeleteEvent(_events[0].Id);

            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task GetTopTenEvents_EmptyCashe_ReturnResultFromRepository()
        {
            //Arrange
            var requieredEvents = _events.ToArray();
            _mockEventsRepository.Setup(repo => repo.GetTopTenEvents()).ReturnsAsync(requieredEvents);
            _mockCasheRepository.Setup(repo => repo.GetTopTenEvents()).ReturnsAsync((Event[]?)null);

            //Act
            var result = await _eventService.GetTopTenEvents();


            //Assert
            _mockCasheRepository.Verify(repo => repo.GetTopTenEvents(), Times.Once);
            _mockEventsRepository.Verify(repo => repo.GetTopTenEvents(), Times.Once);
            _mockCasheRepository.Verify(repo => repo.AddTopTenEventsToCashe(requieredEvents), Times.Once);
            Assert.Equal(result?.Length, requieredEvents.Length);
        }
        [Fact]
        public async Task GetTopTenEvents_NotEmptyCashe_ReturnResultFromCashe()
        {
            //Arrange
            var requieredEvents = _events.ToArray();
            _mockCasheRepository.Setup(repo => repo.GetTopTenEvents()).ReturnsAsync(requieredEvents);

            //Act
            var result = await _eventService.GetTopTenEvents();


            //Assert
            _mockCasheRepository.Verify(repo => repo.GetTopTenEvents(), Times.Once);
            _mockEventsRepository.Verify(repo => repo.GetTopTenEvents(), Times.Never);
            _mockCasheRepository.Verify(repo => repo.AddTopTenEventsToCashe(requieredEvents), Times.Never);
            Assert.Equal(result?.Length, requieredEvents.Length);
        }

    }
}
