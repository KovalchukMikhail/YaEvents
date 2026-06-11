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

namespace ApplicationTests.Services
{
    public class EventServiceTests
    {
        private readonly IEventService _eventService;
        private readonly Mock<IEventsRepository> _mockEventsRepository;
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
            _eventService = new EventService(_mockEventsRepository.Object);
        }

        [Fact]
        public async Task PostEvent_WhenEventAdded_CallRepositoryAdd()
        {
            //Arrange
            var sourceEventDtoLite = new CreateEvent
            {
                Title = _events[0].Title!,
                Description = _events[0].Description!,
                StartAt = _events[0].StartAt,
                EndAt = _events[0].EndAt,
                TotalSeats = _events[0].TotalSeats
            };

            _mockEventsRepository.Setup(m => m.Add(It.IsAny<Event>())).ReturnsAsync(_events[0]);

            //Act
            await _eventService.PostEvent(sourceEventDtoLite);

            //Assert
            _mockEventsRepository.Verify(repo => repo.Add(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task PostEvent_WhenEventAdded_ReturnCorrectEventDto()
        {
            //Arrange
            var sourceEventDtoLite = new CreateEvent
            {
                Title = _events[0].Title!,
                Description = _events[0].Description!,
                StartAt = _events[0].StartAt,
                EndAt = _events[0].EndAt,
                TotalSeats = _events[0].TotalSeats
            };
            _mockEventsRepository.Setup(m => m.Add(It.IsAny<Event>())).ReturnsAsync(_events[0]);

            //Act
            var result = await _eventService.PostEvent(sourceEventDtoLite);

            //Assert
            Assert.True(result?.Title == sourceEventDtoLite.Title
                     && result?.Description == sourceEventDtoLite.Description
                     && result?.StartAt == sourceEventDtoLite.StartAt
                     && result?.EndAt == sourceEventDtoLite.EndAt
                     && result?.Status == EventStatus.Existing);

        }
        [Fact]
        public async Task GetEventsWithPagination_DefaultParameters_CallRepositoryGetFilteredEventsWithPaginationMethod()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.GetFilteredEventsWithPagination()).ReturnsAsync(_events.ToArray());

            //Act
            var result = await _eventService.GetEventsWithPagination();

            //Assert
            _mockEventsRepository.Verify(repo => repo.GetFilteredEventsWithPagination(), Times.Once);
        }
        [Fact]
        public async Task GetEventsWithPagination_DefaultParameters_CallRepositoryGetFilteredEventsCountMethod()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.GetFilteredEventsCount()).ReturnsAsync(10);

            //Act
            var result = await _eventService.GetEventsWithPagination();

            //Assert
            _mockEventsRepository.Verify(repo => repo.GetFilteredEventsCount(), Times.Once);
        }

        [Fact]
        public async Task GetEvent_ExistedId_ReturnsCorrectEventDto()
        {
            //Arrange
            var expectedEvent = _events[0];

            var expectedEventDto = new EventInfo
                (
                    expectedEvent.Id,
                    expectedEvent.Title,
                    expectedEvent.Description,
                    expectedEvent.StartAt,
                    expectedEvent.EndAt,
                    expectedEvent.Status,
                    expectedEvent.TotalSeats,
                    expectedEvent.AvailableSeats
                );
            _mockEventsRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(expectedEvent);

            //Act
            var result = await _eventService.GetEvent(expectedEvent.Id);

            //Assert
            Assert.Equal(result, expectedEventDto);
        }

        [Fact]
        public async Task GetEvent_NoExistedId_ReturnsNull()
        {
            //Arrange

            //Act
            var result = await _eventService.GetEvent(Guid.NewGuid());

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task PutEvent_ExistedId_ReturnTrue()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Update(It.IsAny<Guid>(), It.IsAny<CreateEvent>())).ReturnsAsync(true);

            //Act
            var result = await _eventService.PutEvent(Guid.NewGuid(), new CreateEvent() { Title = "Test", Description = "Test" });

            //Assert
            Assert.True(result);

        }

        [Fact]
        public async Task PutEvent_NoExistedId_ReturnFalse()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Update(It.IsAny<Guid>(), It.IsAny<CreateEvent>())).ReturnsAsync(false);

            //Act
            var result = await _eventService.PutEvent(Guid.NewGuid(), new CreateEvent() { Title = "Test", Description = "Test" });

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteEvent_ExistedId_ReturnTrue()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Delete(It.IsAny<Guid>())).ReturnsAsync(true);

            //Act
            var result = await _eventService.DeleteEvent(Guid.NewGuid());

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteEvent_NoExistedId_ReturnFalse()
        {
            //Arrange
            _mockEventsRepository.Setup(repo => repo.Delete(It.IsAny<Guid>())).ReturnsAsync(false);

            //Act
            var result = await _eventService.DeleteEvent(Guid.NewGuid());

            //Assert
            Assert.False(result);
        }
    }
}
