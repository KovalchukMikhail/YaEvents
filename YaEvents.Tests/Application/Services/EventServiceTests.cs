using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using YaEvents.Application.Services.EventService;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Tests.Application.Services
{
    public class EventServiceTests
    {
        private readonly EventService _eventService;
        private readonly Mock<IRepository<Event>> _mockRepository;
        private readonly List<Event> _events;

        public EventServiceTests()
        {

            _events =
                [
                    new Event{Id = Guid.NewGuid(), Title = "Event001", Description = "Event", StartAt = DateTime.Parse("2000.01.01"), EndAt = DateTime.Parse("2001.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event002", Description = "Event", StartAt = DateTime.Parse("2001.01.01"), EndAt = DateTime.Parse("2002.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event003", Description = "Event", StartAt = DateTime.Parse("2002.01.01"), EndAt = DateTime.Parse("2003.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event004", Description = "Event", StartAt = DateTime.Parse("2003.01.01"), EndAt = DateTime.Parse("2004.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event005", Description = "Event", StartAt = DateTime.Parse("2004.01.01"), EndAt = DateTime.Parse("2005.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event006", Description = "Event", StartAt = DateTime.Parse("2005.01.01"), EndAt = DateTime.Parse("2006.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event007", Description = "Event", StartAt = DateTime.Parse("2006.01.01"), EndAt = DateTime.Parse("2007.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event008", Description = "Event", StartAt = DateTime.Parse("2007.01.01"), EndAt = DateTime.Parse("2008.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event009", Description = "Event", StartAt = DateTime.Parse("2008.01.01"), EndAt = DateTime.Parse("2009.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event010", Description = "Event", StartAt = DateTime.Parse("2009.01.01"), EndAt = DateTime.Parse("2010.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event011", Description = "Event", StartAt = DateTime.Parse("2010.01.01"), EndAt = DateTime.Parse("2011.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100},
                    new Event{Id = Guid.NewGuid(), Title = "Event012", Description = "Event", StartAt = DateTime.Parse("2011.01.01"), EndAt = DateTime.Parse("2012.01.01"), Status = EventStatus.Existing, TotalSeats = 100, AvailableSeats = 100}
                ];

            _mockRepository = new Mock<IRepository<Event>>();
            _eventService = new EventService(_mockRepository.Object);
        }

        [Fact]
        public async Task PostEvent_WhenEventAdded_CallRepositoryAdd()
        {
            //Arrange
            var sourceEventDtoLite = new CreateEvent
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.Parse("2010.01.01"),
                EndAt = DateTime.Parse("2011.01.01")
            };

            _mockRepository.Setup(m => m.Add(It.IsAny<Event>()));

            //Act
            await _eventService.PostEvent(sourceEventDtoLite);


            //Assert
            _mockRepository.Verify(repo => repo.Add(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task PostEvent_WhenEventAdded_ReturnCorrectEventDto()
        {
            //Arrange
            var sourceEventDtoLite = new CreateEvent
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.Parse("2010.01.01"),
                EndAt = DateTime.Parse("2011.01.01")
            };

            _mockRepository.Setup(m => m.Add(It.IsAny<Event>()));

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
        public async Task GetEvents_DefaultParameters_ReturnsAllEvents()
        {
            //Arrange
            _mockRepository.Setup(m => m.GetAll()).ReturnsAsync(_events);
            EventInfo[] expectedResult = _events.Select(e => new EventInfo(e.Id, e.Title, e.Description, e.StartAt, e.EndAt, e.Status, e.TotalSeats, e.AvailableSeats)).ToArray();

            //Act
            var result = await _eventService.GetEvents();

            //Assert
            Assert.Equal(expectedResult, result);
        }
        [Fact]
        public async Task GetEvents_FilteredByTitle_ReturnsCorrectResult()
        {
            //Arrange
            var title = "2";
            _mockRepository.Setup(m => m.GetAll()).ReturnsAsync(_events);
            EventInfo[] expectedResult = 
            [
                new EventInfo(_events[1].Id, "Event002", "Event", DateTime.Parse("2001.01.01"), DateTime.Parse("2002.01.01"), EventStatus.Existing, _events[1].TotalSeats, _events[1].AvailableSeats),
                new EventInfo(_events[11].Id, "Event012", "Event", DateTime.Parse("2011.01.01"), DateTime.Parse("2012.01.01"), EventStatus.Existing, _events[11].TotalSeats, _events[11].AvailableSeats)
            ];

            //Act
            var result = await _eventService.GetEvents(title: title);

            //Assert
            Assert.Equal(expectedResult, result);
        }
        [Fact]
        public async Task GetEvents_FilteredByAllParameters_ReturnsCorrectResult()
        {
            //Arrange
            var title = "01";
            var from = DateTime.Parse("2007.06.01");
            var to = DateTime.Parse("2011.06.01");
            _mockRepository.Setup(m => m.GetAll()).ReturnsAsync(_events);
            EventInfo[] expectedResult =
            [
                new EventInfo(_events[9].Id, "Event010", "Event", DateTime.Parse("2009.01.01"), DateTime.Parse("2010.01.01"), EventStatus.Existing, _events[9].TotalSeats, _events[9].AvailableSeats),
                new EventInfo(_events[10].Id, "Event011", "Event", DateTime.Parse("2010.01.01"), DateTime.Parse("2011.01.01"), EventStatus.Existing, _events[10].TotalSeats, _events[10].AvailableSeats)
            ];

            //Act
            var result = await _eventService.GetEvents(title, from, to);

            //Assert
            Assert.Equal(expectedResult, result);
        }
        [Fact]
        public async Task GetEvents_FilteredByDateFrom_ReturnsCorrectResult()
        {
            //Arrange
            var from = DateTime.Parse("2007.06.01");
            _mockRepository.Setup(m => m.GetAll()).ReturnsAsync(_events);
            EventInfo[] expectedResult =
            [
                new EventInfo(_events[8].Id, "Event009", "Event", DateTime.Parse("2008.01.01"), DateTime.Parse("2009.01.01"), EventStatus.Existing, _events[8].TotalSeats, _events[8].AvailableSeats),
                new EventInfo(_events[9].Id, "Event010", "Event", DateTime.Parse("2009.01.01"), DateTime.Parse("2010.01.01"), EventStatus.Existing, _events[9].TotalSeats, _events[9].AvailableSeats),
                new EventInfo(_events[10].Id, "Event011", "Event", DateTime.Parse("2010.01.01"), DateTime.Parse("2011.01.01"), EventStatus.Existing, _events[10].TotalSeats, _events[10].AvailableSeats),
                new EventInfo(_events[11].Id, "Event012", "Event", DateTime.Parse("2011.01.01"), DateTime.Parse("2012.01.01"), EventStatus.Existing, _events[11].TotalSeats, _events[11].AvailableSeats)
            ];

            //Act
            var result = await _eventService.GetEvents(from: from);

            //Assert
            Assert.Equal(expectedResult, result);
        }
        [Fact]
        public async Task GetEvents_FilteredByDateTo_ReturnsCorrectResult()
        {
            //Arrange
            var to = DateTime.Parse("2004.06.01");
            _mockRepository.Setup(m => m.GetAll()).ReturnsAsync(_events);
            EventInfo[] expectedResult =
            [
                new EventInfo(_events[0].Id, "Event001", "Event", DateTime.Parse("2000.01.01"), DateTime.Parse("2001.01.01"), EventStatus.Existing, _events[0].TotalSeats, _events[0].AvailableSeats),
                new EventInfo(_events[1].Id, "Event002", "Event", DateTime.Parse("2001.01.01"), DateTime.Parse("2002.01.01"), EventStatus.Existing, _events[1].TotalSeats, _events[1].AvailableSeats),
                new EventInfo(_events[2].Id, "Event003", "Event", DateTime.Parse("2002.01.01"), DateTime.Parse("2003.01.01"), EventStatus.Existing, _events[2].TotalSeats, _events[2].AvailableSeats),
                new EventInfo(_events[3].Id, "Event004", "Event", DateTime.Parse("2003.01.01"), DateTime.Parse("2004.01.01"), EventStatus.Existing, _events[3].TotalSeats, _events[3].AvailableSeats)
            ];

            //Act
            var result = await _eventService.GetEvents(to: to);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetEvent_ExistedId_ReturnsCorrectEventDto()
        {
            //Arrange
            var expectedEvent = new Event
            {
                Id = _events[0].Id,
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.Parse("2010.01.01"),
                EndAt = DateTime.Parse("2011.01.01"),
                Status = EventStatus.Existing,
                TotalSeats = 100,
                AvailableSeats = 100
            };
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

            _mockRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(expectedEvent);

            //Act
            var result = await _eventService.GetEvent(_events[0].Id);

            //Assert
            Assert.Equal(result, expectedEventDto);
        }

        [Fact]
        public async Task GetEvent_NoExistedId_ReturnsNull()
        {
            //Arrange
            _mockRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync((Event?)null);

            //Act
            var result = await _eventService.GetEvent(_events[0].Id);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task PutEvent_ExistedId_ReturnTrue()
        {
            //Arrange
            _mockRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync(new Event() { Title = "Test", Description = "Test" });

            //Act
            var result = await _eventService.PutEvent(_events[0].Id, new CreateEvent() { Title = "Test", Description = "Test" });

            //Assert
            Assert.True(result);

        }

        [Fact]
        public async Task PutEvent_NoExistedId_ReturnFalse()
        {
            //Arrange
            _mockRepository.Setup(m => m.Get(It.IsAny<Guid>())).ReturnsAsync((Event?)null);

            //Act
            var result = await _eventService.PutEvent(_events[0].Id, new CreateEvent() { Title = "Test", Description = "Test" });

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteEvent_ExistedId_ReturnTrue()
        {
            //Arrange
            var id = _events[0].Id;
            _mockRepository.Setup(m => m.Delete(id)).ReturnsAsync(true);

            //Act
            var result = await _eventService.DeleteEvent(id);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteEvent_NoExistedId_ReturnFalse()
        {
            //Arrange
            var id = _events[0].Id;
            _mockRepository.Setup(m => m.Delete(id)).ReturnsAsync(false);

            //Act
            var result =  await _eventService.DeleteEvent(id);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetEventsWithPagination_ReturnsCorrectResult()
        {
            //Arrange
            var sourceEvents = _events.Select(e => new EventInfo(e.Id, e.Title, e.Description, e.StartAt, e.EndAt, e.Status, e.TotalSeats, e.AvailableSeats)).ToArray();
            var pageNumber = 2;
            var pageSize = 3;
            EventInfo[] expectedItems =
            [
                new EventInfo(_events[3].Id, "Event004", "Event", DateTime.Parse("2003.01.01"), DateTime.Parse("2004.01.01"), EventStatus.Existing, _events[3].TotalSeats, _events[3].AvailableSeats),
                new EventInfo(_events[4].Id, "Event005", "Event", DateTime.Parse("2004.01.01"), DateTime.Parse("2005.01.01"), EventStatus.Existing, _events[4].TotalSeats, _events[4].AvailableSeats),
                new EventInfo(_events[5].Id, "Event006", "Event", DateTime.Parse("2005.01.01"), DateTime.Parse("2006.01.01"), EventStatus.Existing, _events[5].TotalSeats, _events[5].AvailableSeats)
            ];

            var expectedResult = new PaginatedResult<EventInfo>(expectedItems, pageNumber, 4, 3, 12);

            //Act
            var result = await _eventService.GetEventsWithPagination(sourceEvents, pageNumber, pageSize);

            //Assert
            Assert.Equal(expectedResult.Items, result?.Items);
            Assert.True(expectedResult.CurrentPageItemsCount == result?.CurrentPageItemsCount
                        && expectedResult.TotalPages == result?.TotalPages
                        && expectedResult.CurrentPage == result?.CurrentPage
                        && expectedResult.TotalItems == result?.TotalItems
                        );

        }


    }
}
