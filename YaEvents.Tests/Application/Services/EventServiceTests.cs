using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using YaEvents.Application.Services.EventService;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
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
                    new Event{Id = 1, Title = "Event001", Description = "Event", StartAt = DateTime.Parse("2000.01.01"), EndAt = DateTime.Parse("2001.01.01")},
                    new Event{Id = 2, Title = "Event002", Description = "Event", StartAt = DateTime.Parse("2001.01.01"), EndAt = DateTime.Parse("2002.01.01")},
                    new Event{Id = 3, Title = "Event003", Description = "Event", StartAt = DateTime.Parse("2002.01.01"), EndAt = DateTime.Parse("2003.01.01")},
                    new Event{Id = 4, Title = "Event004", Description = "Event", StartAt = DateTime.Parse("2003.01.01"), EndAt = DateTime.Parse("2004.01.01")},
                    new Event{Id = 5, Title = "Event005", Description = "Event", StartAt = DateTime.Parse("2004.01.01"), EndAt = DateTime.Parse("2005.01.01")},
                    new Event{Id = 6, Title = "Event006", Description = "Event", StartAt = DateTime.Parse("2005.01.01"), EndAt = DateTime.Parse("2006.01.01")},
                    new Event{Id = 7, Title = "Event007", Description = "Event", StartAt = DateTime.Parse("2006.01.01"), EndAt = DateTime.Parse("2007.01.01")},
                    new Event{Id = 8, Title = "Event008", Description = "Event", StartAt = DateTime.Parse("2007.01.01"), EndAt = DateTime.Parse("2008.01.01")},
                    new Event{Id = 9, Title = "Event009", Description = "Event", StartAt = DateTime.Parse("2008.01.01"), EndAt = DateTime.Parse("2009.01.01")},
                    new Event{Id = 10, Title = "Event010", Description = "Event", StartAt = DateTime.Parse("2009.01.01"), EndAt = DateTime.Parse("2010.01.01")},
                    new Event{Id = 11, Title = "Event011", Description = "Event", StartAt = DateTime.Parse("2010.01.01"), EndAt = DateTime.Parse("2011.01.01")},
                    new Event{Id = 12, Title = "Event012", Description = "Event", StartAt = DateTime.Parse("2011.01.01"), EndAt = DateTime.Parse("2012.01.01")}
                ];

            _mockRepository = new Mock<IRepository<Event>>();
            _eventService = new EventService(_mockRepository.Object);
        }

        [Fact]
        public void PostEvent_WhenEventAdded_CallRepositoryAddMethodWithCorrectParameter()
        {
            //Arrange
            var sourceEventDtoLite = new EventDtoLite
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.Parse("2010.01.01"),
                EndAt = DateTime.Parse("2011.01.01")
            };

            var expectedParamForRepositoryAddMethod = new Event
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.Parse("2010.01.01"),
                EndAt = DateTime.Parse("2011.01.01")
            };

            _mockRepository.Setup(m => m.Add(It.IsAny<Event>())).Returns(new Event() { Title = "Test", Description = "Test"});

            //Act
            _eventService.PostEvent(sourceEventDtoLite);


            //Assert
            _mockRepository.Verify(repo => repo.Add(expectedParamForRepositoryAddMethod), Times.Once);

        }

        [Fact]
        public void PostEvent_WhenEventAdded_ReturnCorrectEventDto()
        {
            //Arrange
            var newEvent = new Event
            {
                Id = 1,
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.Parse("2010.01.01"),
                EndAt = DateTime.Parse("2011.01.01")
            };
            var expectedEventDto = new EventDto(newEvent.Id, newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt);

            _mockRepository.Setup(m => m.Add(It.IsAny<Event>())).Returns(newEvent);

            //Act
            var result = _eventService.PostEvent(new EventDtoLite() { Title = "Test", Description = "Test"} );


            //Assert
            Assert.Equal(result, expectedEventDto);
        }

        [Fact]
        public void GetEvents_DefaultParameters_ReturnsAllEvents()
        {
            //Arrange
            _mockRepository.Setup(m => m.GetAll()).Returns(_events);
            EventDto[] expectedResult = _events.Select(e => new EventDto(e.Id, e.Title, e.Description, e.StartAt, e.EndAt)).ToArray();

            //Act
            var result = _eventService.GetEvents();

            //Assert
            Assert.Equal(expectedResult, result);
        }
        [Fact]
        public void GetEvents_FilteredByTitle_ReturnsCorrectResult()
        {
            //Arrange
            var title = "2";
            _mockRepository.Setup(m => m.GetAll()).Returns(_events);
            EventDto[] expectedResult = 
            [
                new EventDto(2, "Event002", "Event", DateTime.Parse("2001.01.01"), DateTime.Parse("2002.01.01")),
                new EventDto(12, "Event012", "Event", DateTime.Parse("2011.01.01"), DateTime.Parse("2012.01.01"))
            ];

            //Act
            var result = _eventService.GetEvents(title: title);

            //Assert
            Assert.Equal(expectedResult, result);
        }
        [Fact]
        public void GetEvents_FilteredByAllParameters_ReturnsCorrectResult()
        {
            //Arrange
            var title = "01";
            var from = DateTime.Parse("2007.06.01");
            var to = DateTime.Parse("2011.06.01");
            _mockRepository.Setup(m => m.GetAll()).Returns(_events);
            EventDto[] expectedResult =
            [
                new EventDto(10, "Event010", "Event", DateTime.Parse("2009.01.01"), DateTime.Parse("2010.01.01")),
                new EventDto(11, "Event011", "Event", DateTime.Parse("2010.01.01"), DateTime.Parse("2011.01.01"))
            ];

            //Act
            var result = _eventService.GetEvents(title, from, to);

            //Assert
            Assert.Equal(expectedResult, result);
        }
        [Fact]
        public void GetEvents_FilteredByDateFrom_ReturnsCorrectResult()
        {
            //Arrange
            var from = DateTime.Parse("2007.06.01");
            _mockRepository.Setup(m => m.GetAll()).Returns(_events);
            EventDto[] expectedResult =
            [
                new EventDto(9, "Event009", "Event", DateTime.Parse("2008.01.01"), DateTime.Parse("2009.01.01")),
                new EventDto(10, "Event010", "Event", DateTime.Parse("2009.01.01"), DateTime.Parse("2010.01.01")),
                new EventDto(11, "Event011", "Event", DateTime.Parse("2010.01.01"), DateTime.Parse("2011.01.01")),
                new EventDto(12, "Event012", "Event", DateTime.Parse("2011.01.01"), DateTime.Parse("2012.01.01"))
            ];

            //Act
            var result = _eventService.GetEvents(from: from);

            //Assert
            Assert.Equal(expectedResult, result);
        }
        [Fact]
        public void GetEvents_FilteredByDateTo_ReturnsCorrectResult()
        {
            //Arrange
            var to = DateTime.Parse("2004.06.01");
            _mockRepository.Setup(m => m.GetAll()).Returns(_events);
            EventDto[] expectedResult =
            [
                new EventDto(1, "Event001", "Event", DateTime.Parse("2000.01.01"), DateTime.Parse("2001.01.01")),
                new EventDto(2, "Event002", "Event", DateTime.Parse("2001.01.01"), DateTime.Parse("2002.01.01")),
                new EventDto(3, "Event003", "Event", DateTime.Parse("2002.01.01"), DateTime.Parse("2003.01.01")),
                new EventDto(4, "Event004", "Event", DateTime.Parse("2003.01.01"), DateTime.Parse("2004.01.01"))
            ];

            //Act
            var result = _eventService.GetEvents(to: to);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetEvent_ExistedId_ReturnsCorrectEventDto()
        {
            //Arrange
            var expectedEvent = new Event
            {
                Id = 1,
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.Parse("2010.01.01"),
                EndAt = DateTime.Parse("2011.01.01")
            };
            var expectedEventDto = new EventDto
                (
                    expectedEvent.Id,
                    expectedEvent.Title,
                    expectedEvent.Description,
                    expectedEvent.StartAt,
                    expectedEvent.EndAt
                );

            _mockRepository.Setup(m => m.Get(It.IsAny<int>())).Returns(expectedEvent);

            //Act
            var result = _eventService.GetEvent(1);

            //Assert
            Assert.Equal(result, expectedEventDto);
        }

        [Fact]
        public void GetEvent_NoExistedId_ReturnsNull()
        {
            //Arrange
            _mockRepository.Setup(m => m.Get(It.IsAny<int>())).Returns((Event?)null);

            //Act
            var result = _eventService.GetEvent(1);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public void PutEvent_ExistedId_ReturnTrue()
        {
            //Arrange
            _mockRepository.Setup(m => m.Get(It.IsAny<int>())).Returns(new Event() { Title = "Test", Description = "Test" });

            //Act
            var result = _eventService.PutEvent(1, new EventDtoLite() { Title = "Test", Description = "Test" });

            //Assert
            Assert.True(result);

        }

        [Fact]
        public void PutEvent_NoExistedId_ReturnFalse()
        {
            //Arrange
            _mockRepository.Setup(m => m.Get(It.IsAny<int>())).Returns((Event?)null);

            //Act
            var result = _eventService.PutEvent(1, new EventDtoLite() { Title = "Test", Description = "Test" });

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void DeleteEvent_ExistedId_ReturnTrue()
        {
            //Arrange
            var id = 1;
            _mockRepository.Setup(m => m.Delete(id)).Returns(true);

            //Act
            var result = _eventService.DeleteEvent(id);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void DeleteEvent_NoExistedId_ReturnFalse()
        {
            //Arrange
            var id = 1;
            _mockRepository.Setup(m => m.Delete(id)).Returns(false);

            //Act
            var result = _eventService.DeleteEvent(id);

            //Assert
            Assert.False(result);
        }
        [Fact]
        public void GetEventsWithPagination_ReturnsCorrectResult()
        {
            //Arrange
            var sourceEvents = _events.Select(e => new EventDto(e.Id, e.Title, e.Description, e.StartAt, e.EndAt)).ToArray();
            var pageNumber = 2;
            var pageSize = 3;
            EventDto[] expectedItems =
            [
                new EventDto(4, "Event004", "Event", DateTime.Parse("2003.01.01"), DateTime.Parse("2004.01.01")),
                new EventDto(5, "Event005", "Event", DateTime.Parse("2004.01.01"), DateTime.Parse("2005.01.01")),
                new EventDto(6, "Event006", "Event", DateTime.Parse("2005.01.01"), DateTime.Parse("2006.01.01"))
            ];

            var expectedResult = new PaginatedResult<EventDto>(expectedItems, pageNumber, 4, 3, 12);

            //Act
            var result = _eventService.GetEventsWithPagination(sourceEvents, pageNumber, pageSize);

            //Assert
            Assert.Equal(expectedResult.Items, result.Items);
            Assert.True(expectedResult.CurrentPageItemsCount == result.CurrentPageItemsCount
                        && expectedResult.TotalPages == result.TotalPages
                        && expectedResult.CurrentPage == result.CurrentPage
                        && expectedResult.TotalItems == result.TotalItems
                        );

        }


    }
}
