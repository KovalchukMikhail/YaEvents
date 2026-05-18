using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Application.Services.EventService
{
    public class EventService : IEventService
    {
        public static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _eventSemaphores = new ConcurrentDictionary<Guid, SemaphoreSlim>();

        protected readonly AppDbContext _appDbContext;
        public EventService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<EventInfo[]> GetEvents(string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default)
        {
            var events = _appDbContext.Events.Where(e => e.Status != EventStatus.Removed);
            title = title?.Trim();

            if (!string.IsNullOrEmpty(title))
                events = events.Where(e => !string.IsNullOrEmpty(e.Title) && e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if(from != null)
                events = events.Where(e => e.StartAt >= from);

            if (to != null)
                events = events.Where(e => e.EndAt <= to);

            return await events.Select(e => new EventInfo(e.Id, e.Title, e.Description, e.StartAt, e.EndAt, e.Status, e.TotalSeats, e.AvailableSeats))
                         .ToArrayAsync();
        }
        public async Task<EventInfo?> GetEvent(Guid id, CancellationToken token = default)
        {
            var requiredEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id && e.Status != EventStatus.Removed);
            if (requiredEvent != null)
            {
                return new EventInfo(requiredEvent.Id, requiredEvent.Title, requiredEvent.Description, requiredEvent.StartAt, requiredEvent.EndAt, requiredEvent.Status, requiredEvent.TotalSeats, requiredEvent.AvailableSeats);
            }
            else
                return null;
        }

        public async Task<EventInfo> PostEvent(CreateEvent createEvent, CancellationToken token = default)
        {
            var newEvent = new Event
                (Guid.NewGuid(),
                createEvent.Title,
                createEvent.Description,
                createEvent.StartAt,
                createEvent.EndAt,
                EventStatus.Existing,
                createEvent.TotalSeats,
                createEvent.TotalSeats);

            await _appDbContext.Events.AddAsync(newEvent, token);
            await _appDbContext.SaveChangesAsync(token);

            return new EventInfo(newEvent.Id, newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt, newEvent.Status, newEvent.TotalSeats, newEvent.AvailableSeats);
        }

        public async Task<bool> PutEvent(Guid id, CreateEvent createEvent, CancellationToken token = default)
        {
            var requiredEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id && e.Status != EventStatus.Removed, token);
            if (requiredEvent == null)
                return false;

            var eventSemaphore = AppSemaphores.GetSemaphore(requiredEvent.Id);
            await eventSemaphore.WaitAsync();
            try
            {
                var bookedSeats = requiredEvent.TotalSeats - requiredEvent.AvailableSeats;
                if (bookedSeats > createEvent.TotalSeats)
                    throw new ValidationException("Количество мест в измененном событии, меньше чем количество уже забронированных мест.");

                requiredEvent.Title = createEvent.Title;
                requiredEvent.Description = createEvent.Description;
                requiredEvent.StartAt = createEvent.StartAt;
                requiredEvent.EndAt = createEvent.EndAt;
                requiredEvent.TotalSeats = createEvent.TotalSeats;
                requiredEvent.AvailableSeats = createEvent.TotalSeats - bookedSeats;

                await _appDbContext.SaveChangesAsync(token);

                return true;
            }
            finally
            {
                eventSemaphore.Release();
            }
        }
        public async Task<bool> DeleteEvent(Guid id, CancellationToken token = default)
        {
            var requiredEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id && e.Status != EventStatus.Removed);
            if (requiredEvent == null)
                return false;
            else
            {
                requiredEvent.Status = EventStatus.Removed;
                await _appDbContext.SaveChangesAsync(token);

                return true;
            }
        }
        public async Task<PaginatedResult<EventInfo>> GetEventsWithPagination(EventInfo[] sourceEvents, int pageNumber, int pageSize, CancellationToken token = default)
        {
            var events = sourceEvents.Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToArray();

            int totalPages = (int)Math.Ceiling((double)sourceEvents.Length / pageSize);

            return new PaginatedResult<EventInfo>(events, pageNumber, totalPages, events.Length, sourceEvents.Length);
        }
    }
}
