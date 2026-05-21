using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Repositories.EventsRepository;

namespace YaEvents.IntegrationTests
{
    public class EventsRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
                                                    .Build();
        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        private async Task<AppDbContext> CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                                .UseNpgsql(_postgres.GetConnectionString())
                                .Options;

            var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            await using var context = await CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE bookings, events RESTART IDENTITY CASCADE");
        }
        private Event CreateEvent(string? title = null, string? Description = null, DateTime? startAt = null, DateTime? endAt = null, EventStatus? status = null, int? totalSeats = null, int? availableSeats = null)
        {
            return new Event
                (
                    Guid.NewGuid(),
                    title ?? "Title",
                    Description ?? "Description",
                    startAt ?? DateTime.Parse("2010.01.01").ToUniversalTime(),
                    endAt ?? DateTime.Parse("2011.01.01").ToUniversalTime(),
                    status ?? Infrastructure.Enums.EventStatus.Existing,
                    totalSeats ?? 4,
                    availableSeats ?? 4
                );
        }

        [Fact]
        public async Task Add_CorrectParameters_SaveEventToDataBase()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var eventRepository = new EventsRepository(context);
            var @event = CreateEvent();

            //Act
            await eventRepository.Add(@event);

            //Assert
            context = await CreateContext();
            Assert.Equal(@event, context.Events.FirstOrDefault(e => e.Id == @event.Id));
        }
        [Fact]
        public async Task Update_CorrectParameters_UpdatedEventInDataBase()
        {
            //Arrange
            var titleBeforeUpdate = "Test001";
            var titleAfterUpdate = "Test002";
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var @event = CreateEvent(title: titleBeforeUpdate);
            await context.Events.AddAsync(@event);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);
            var createEvent = new CreateEvent() { Description = @event.Description!, Title = titleAfterUpdate, StartAt = @event.StartAt, EndAt = @event.EndAt, TotalSeats = @event.TotalSeats };

            //Act
            await eventRepository.Update(@event.Id, createEvent);

            //Assert
            context = await CreateContext();
            var updatedEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == @event.Id);
            Assert.Equal(titleAfterUpdate, updatedEvent?.Title);
        }

        [Fact]
        public async Task Delete_CorrectParameters_EventWithStatusRemovedInDataBase()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var @event = CreateEvent();
            await context.Events.AddAsync(@event);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            await eventRepository.Delete(@event.Id);

            //Assert
            context = await CreateContext();
            var removedEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == @event.Id);
            Assert.Equal(EventStatus.Removed, removedEvent?.Status);
        }
        [Fact]
        public async Task Get_CorrectParameters_GetEventFromDataBase()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var @event = CreateEvent();
            await context.Events.AddAsync(@event);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var curEvent = await eventRepository.Get(@event.Id);

            //Assert
            Assert.NotNull(curEvent);
        }
        [Fact]
        public async Task GetFilteredEventsWithPagination_DefaultParameters_GetAllEvents()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var event1 = CreateEvent(title: "001", startAt: DateTime.Parse("2010.01.01").ToUniversalTime(), endAt: DateTime.Parse("2010.01.05").ToUniversalTime());
            var event2 = CreateEvent(title: "002", startAt: DateTime.Parse("2011.01.01").ToUniversalTime(), endAt: DateTime.Parse("2011.01.05").ToUniversalTime());
            var event3 = CreateEvent(title: "011", startAt: DateTime.Parse("2012.01.01").ToUniversalTime(), endAt: DateTime.Parse("2012.01.05").ToUniversalTime());
            await context.Events.AddAsync(event1);
            await context.Events.AddAsync(event2);
            await context.Events.AddAsync(event3);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var result = await eventRepository.GetFilteredEventsWithPagination();

            //Assert
            Assert.Equal(3, result.Length);
            Assert.Contains(result, e => e.Title == "001");
            Assert.Contains(result, e => e.Title == "002");
            Assert.Contains(result, e => e.Title == "011");
        }
        [Fact]
        public async Task GetFilteredEventsWithPagination_FilteredByTitle_GetReqieredEvents()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var event1 = CreateEvent(title: "001", startAt: DateTime.Parse("2010.01.01").ToUniversalTime(), endAt: DateTime.Parse("2010.01.05").ToUniversalTime());
            var event2 = CreateEvent(title: "002", startAt: DateTime.Parse("2011.01.01").ToUniversalTime(), endAt: DateTime.Parse("2011.01.05").ToUniversalTime());
            var event3 = CreateEvent(title: "011", startAt: DateTime.Parse("2012.01.01").ToUniversalTime(), endAt: DateTime.Parse("2012.01.05").ToUniversalTime());
            await context.Events.AddAsync(event1);
            await context.Events.AddAsync(event2);
            await context.Events.AddAsync(event3);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var result = await eventRepository.GetFilteredEventsWithPagination(title: "01");

            //Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(result, e => e.Title == "001");
            Assert.Contains(result, e => e.Title == "011");
        }
        [Fact]
        public async Task GetFilteredEventsWithPagination_FilteredByFrom_GetReqieredEvents()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var event1 = CreateEvent(title: "001", startAt: DateTime.Parse("2010.01.01").ToUniversalTime(), endAt: DateTime.Parse("2010.01.05").ToUniversalTime());
            var event2 = CreateEvent(title: "002", startAt: DateTime.Parse("2011.01.01").ToUniversalTime(), endAt: DateTime.Parse("2011.01.05").ToUniversalTime());
            var event3 = CreateEvent(title: "011", startAt: DateTime.Parse("2012.01.01").ToUniversalTime(), endAt: DateTime.Parse("2012.01.05").ToUniversalTime());
            await context.Events.AddAsync(event1);
            await context.Events.AddAsync(event2);
            await context.Events.AddAsync(event3);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var result = await eventRepository.GetFilteredEventsWithPagination(from: DateTime.Parse("2010.06.01").ToUniversalTime());

            //Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(result, e => e.Title == "002");
            Assert.Contains(result, e => e.Title == "011");
        }
        [Fact]
        public async Task GetFilteredEventsWithPagination_FilteredByTo_GetReqieredEvents()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var event1 = CreateEvent(title: "001", startAt: DateTime.Parse("2010.01.01").ToUniversalTime(), endAt: DateTime.Parse("2010.01.05").ToUniversalTime());
            var event2 = CreateEvent(title: "002", startAt: DateTime.Parse("2011.01.01").ToUniversalTime(), endAt: DateTime.Parse("2011.01.05").ToUniversalTime());
            var event3 = CreateEvent(title: "011", startAt: DateTime.Parse("2012.01.01").ToUniversalTime(), endAt: DateTime.Parse("2012.01.05").ToUniversalTime());
            await context.Events.AddAsync(event1);
            await context.Events.AddAsync(event2);
            await context.Events.AddAsync(event3);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var result = await eventRepository.GetFilteredEventsWithPagination(to: DateTime.Parse("2011.06.01").ToUniversalTime());

            //Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(result, e => e.Title == "001");
            Assert.Contains(result, e => e.Title == "002");
        }
        [Fact]
        public async Task GetFilteredEventsWithPagination_FilteredByAllFilteres_GetReqieredEvents()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var event1 = CreateEvent(title: "001", startAt: DateTime.Parse("2010.01.01").ToUniversalTime(), endAt: DateTime.Parse("2010.01.05").ToUniversalTime());
            var event2 = CreateEvent(title: "002", startAt: DateTime.Parse("2011.01.01").ToUniversalTime(), endAt: DateTime.Parse("2011.01.05").ToUniversalTime());
            var event3 = CreateEvent(title: "011", startAt: DateTime.Parse("2012.01.01").ToUniversalTime(), endAt: DateTime.Parse("2012.01.05").ToUniversalTime());
            var event4 = CreateEvent(title: "012", startAt: DateTime.Parse("2012.01.03").ToUniversalTime(), endAt: DateTime.Parse("2012.01.08").ToUniversalTime());
            var event5 = CreateEvent(title: "012", startAt: DateTime.Parse("2022.01.01").ToUniversalTime(), endAt: DateTime.Parse("2022.01.05").ToUniversalTime());
            await context.Events.AddAsync(event1);
            await context.Events.AddAsync(event2);
            await context.Events.AddAsync(event3);
            await context.Events.AddAsync(event4);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var result = await eventRepository.GetFilteredEventsWithPagination(title: "01", from: DateTime.Parse("2010.05.01").ToUniversalTime(), to: DateTime.Parse("2013.06.01").ToUniversalTime());

            //Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(result, e => e.Title == "011");
            Assert.Contains(result, e => e.Title == "012");
        }
        [Fact]
        public async Task GetFilteredEventsWithPagination_ReqierOnlySecondPage_GetReqieredEvents()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var event1 = CreateEvent(title: "001", startAt: DateTime.Parse("2010.01.01").ToUniversalTime(), endAt: DateTime.Parse("2010.01.05").ToUniversalTime());
            var event2 = CreateEvent(title: "002", startAt: DateTime.Parse("2011.01.01").ToUniversalTime(), endAt: DateTime.Parse("2011.01.05").ToUniversalTime());
            var event3 = CreateEvent(title: "011", startAt: DateTime.Parse("2012.01.01").ToUniversalTime(), endAt: DateTime.Parse("2012.01.05").ToUniversalTime());
            await context.Events.AddAsync(event1);
            await context.Events.AddAsync(event2);
            await context.Events.AddAsync(event3);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var result = await eventRepository.GetFilteredEventsWithPagination(pageNumber: 2, pageSize: 2);

            //Assert
            Assert.True(result.Length == 1);
            Assert.Equal("011", result[0].Title);
        }
        [Fact]
        public async Task GetFilteredEventsCount_DefaultParameters_GetCorrectCount()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var event1 = CreateEvent(title: "001", startAt: DateTime.Parse("2010.01.01").ToUniversalTime(), endAt: DateTime.Parse("2010.01.05").ToUniversalTime());
            var event2 = CreateEvent(title: "002", startAt: DateTime.Parse("2011.01.01").ToUniversalTime(), endAt: DateTime.Parse("2011.01.05").ToUniversalTime());
            var event3 = CreateEvent(title: "011", startAt: DateTime.Parse("2012.01.01").ToUniversalTime(), endAt: DateTime.Parse("2012.01.05").ToUniversalTime());
            await context.Events.AddAsync(event1);
            await context.Events.AddAsync(event2);
            await context.Events.AddAsync(event3);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var result = await eventRepository.GetFilteredEventsCount();

            //Assert
            Assert.Equal(3, result);
        }
        [Fact]
        public async Task GetFilteredEventsCount_FilteredByTitle_GetCorrectCount()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var event1 = CreateEvent(title: "001", startAt: DateTime.Parse("2010.01.01").ToUniversalTime(), endAt: DateTime.Parse("2010.01.05").ToUniversalTime());
            var event2 = CreateEvent(title: "002", startAt: DateTime.Parse("2011.01.01").ToUniversalTime(), endAt: DateTime.Parse("2011.01.05").ToUniversalTime());
            var event3 = CreateEvent(title: "011", startAt: DateTime.Parse("2012.01.01").ToUniversalTime(), endAt: DateTime.Parse("2012.01.05").ToUniversalTime());
            await context.Events.AddAsync(event1);
            await context.Events.AddAsync(event2);
            await context.Events.AddAsync(event3);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var result = await eventRepository.GetFilteredEventsCount(title: "01");

            //Assert
            Assert.Equal(2, result);

        }
        [Fact]
        public async Task GetFilteredEventsCount_FilteredByFrom_GetCorrectCount()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var event1 = CreateEvent(title: "001", startAt: DateTime.Parse("2010.01.01").ToUniversalTime(), endAt: DateTime.Parse("2010.01.05").ToUniversalTime());
            var event2 = CreateEvent(title: "002", startAt: DateTime.Parse("2011.01.01").ToUniversalTime(), endAt: DateTime.Parse("2011.01.05").ToUniversalTime());
            var event3 = CreateEvent(title: "011", startAt: DateTime.Parse("2012.01.01").ToUniversalTime(), endAt: DateTime.Parse("2012.01.05").ToUniversalTime());
            await context.Events.AddAsync(event1);
            await context.Events.AddAsync(event2);
            await context.Events.AddAsync(event3);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var result = await eventRepository.GetFilteredEventsCount(from: DateTime.Parse("2010.06.01").ToUniversalTime());

            //Assert
            Assert.Equal(2, result);
        }
        [Fact]
        public async Task GetFilteredEventsCount_FilteredByTo_GetCorrectCount()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var event1 = CreateEvent(title: "001", startAt: DateTime.Parse("2010.01.01").ToUniversalTime(), endAt: DateTime.Parse("2010.01.05").ToUniversalTime());
            var event2 = CreateEvent(title: "002", startAt: DateTime.Parse("2011.01.01").ToUniversalTime(), endAt: DateTime.Parse("2011.01.05").ToUniversalTime());
            var event3 = CreateEvent(title: "011", startAt: DateTime.Parse("2012.01.01").ToUniversalTime(), endAt: DateTime.Parse("2012.01.05").ToUniversalTime());
            await context.Events.AddAsync(event1);
            await context.Events.AddAsync(event2);
            await context.Events.AddAsync(event3);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            var result = await eventRepository.GetFilteredEventsCount(to: DateTime.Parse("2011.06.01").ToUniversalTime());

            //Assert
            Assert.Equal(2, result);
        }
        [Fact]
        public async Task ReleaseSeats_CorrectParameters_UpdatedEventInDataBase()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var @event = CreateEvent(totalSeats: 3, availableSeats: 2);
            await context.Events.AddAsync(@event);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            await eventRepository.ReleaseSeats(@event.Id);

            //Assert
            context = await CreateContext();
            var updatedEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == @event.Id);
            Assert.Equal(3, updatedEvent?.AvailableSeats);
        }
        [Fact]
        public async Task TryReserveSeats_CorrectParameters_UpdatedEventInDataBase()
        {
            //Arrange
            await ResetDatabaseAsync();
            var context = await CreateContext();
            var @event = CreateEvent(totalSeats: 3, availableSeats: 2);
            await context.Events.AddAsync(@event);
            await context.SaveChangesAsync();

            context = await CreateContext();
            var eventRepository = new EventsRepository(context);

            //Act
            await eventRepository.TryReserveSeats(@event.Id);

            //Assert
            context = await CreateContext();
            var updatedEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == @event.Id);
            Assert.Equal(1, updatedEvent?.AvailableSeats);
        }
    }
}


