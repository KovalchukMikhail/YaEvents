using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using YaEvents.Application.Services.EventService;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Tests.Application.Services
{
    public class EventServiceTests
    {
        private readonly IEventService _eventService;
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        //private readonly Mock<IRepository<Event>> _mockRepository;
        private readonly List<Event> _events;

        public EventServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventService, EventService>();

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

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            var appDbContext = _scope.ServiceProvider.GetService<AppDbContext>();
            appDbContext.Events.AddRange(_events);
            appDbContext.SaveChanges();

            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();

            

            //_mockRepository = new Mock<IRepository<Event>>();
            //_eventService = new EventService(_mockRepository.Object);
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        //[Fact]
        //public async Task PostEvent_WhenEventAdded_CallRepositoryAdd()
        //{
        //    //Arrange
        //    var sourceEventDtoLite = new CreateEvent
        //    {
        //        Title = "Title",
        //        Description = "Description",
        //        StartAt = DateTime.Parse("2010.01.01"),
        //        EndAt = DateTime.Parse("2011.01.01"),
        //        TotalSeats = 3
        //    };
        //
        //    _mockRepository.Setup(m => m.Add(It.IsAny<Event>()));
        //
        //    //Act
        //    await _eventService.PostEvent(sourceEventDtoLite);
        //
        //
        //    //Assert
        //    _mockRepository.Verify(repo => repo.Add(It.IsAny<Event>()), Times.Once);
        //}

        [Fact]
        public async Task PostEvent_WhenEventAdded_ReturnCorrectEventDto()
        {
            //Arrange
            var sourceEventDtoLite = new CreateEvent
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.Parse("2010.01.01"),
                EndAt = DateTime.Parse("2011.01.01"),
                TotalSeats = 3
            };

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

            //Act
            var result = await _eventService.PutEvent(_events[0].Id, new CreateEvent() { Title = "Test", Description = "Test" });

            //Assert
            Assert.True(result);

        }

        [Fact]
        public async Task PutEvent_NoExistedId_ReturnFalse()
        {
            //Arrange

            //Act
            var result = await _eventService.PutEvent(Guid.NewGuid(), new CreateEvent() { Title = "Test", Description = "Test" });

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteEvent_ExistedId_ReturnTrue()
        {
            //Arrange
            var id = _events[0].Id;

            //Act
            var result = await _eventService.DeleteEvent(id);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteEvent_NoExistedId_ReturnFalse()
        {
            //Arrange
            var id = Guid.NewGuid();

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
