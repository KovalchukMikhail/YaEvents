using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Infrastructure.Repositories.EventsRepository
{
    public class EventsRepository : IEventsRepository
    {
        protected readonly AppDbContext _appDbContext;

        public EventsRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<Event> Add(Event curEvent, CancellationToken token = default)
        {
            await _appDbContext.Events.AddAsync(curEvent, token);
            await _appDbContext.SaveChangesAsync(token);

            return curEvent;
        }

        public async Task<bool> Update(Guid id, CreateEvent createEvent, CancellationToken token = default)
        {
            var requiredEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id && e.Status != EventStatus.Removed, token);
            if (requiredEvent == null)
                return false;

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

        public async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            var @event = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id, token);
            if (@event != null)
            {
                if(@event.Status == Enums.EventStatus.Removed)
                    return false;
                else
                {
                    @event.Status = Enums.EventStatus.Removed;
                    await _appDbContext.SaveChangesAsync(token);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task<Event?> Get(Guid id, CancellationToken token = default)
        {
            return await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id);
        }
        public async Task<int> GetFilteredEventsCount(string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default)
        {
            return await GetFilteredEvents(title, from, to).CountAsync();
        }
        public async Task<Event[]> GetFilteredEventsWithPagination(int pageNumber = 1, int pageSize = 10, string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default)
        {
            return await GetFilteredEvents(title, from, to).OrderBy(e => e.Title).Skip((pageNumber - 1) * pageSize)
                                                            .Take(pageSize)
                                                            .ToArrayAsync();
        }
        public async Task ReleaseSeats(Guid id, CancellationToken token = default)
        {
            var @event = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id, token);

            if (@event == null)
                return;

            @event.ReleaseSeats();
            await _appDbContext.SaveChangesAsync();
        }
        public async Task<bool> TryReserveSeats(Guid id, CancellationToken token = default)
        {
            var @event = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id, token);

            if (@event == null || !@event.TryReserveSeats())
                return false;

            await _appDbContext.SaveChangesAsync();
            return true;
        }
        private IQueryable<Event> GetFilteredEvents(string? title = null, DateTime? from = null, DateTime? to = null)
        {
            var events = _appDbContext.Events.Where(e => e.Status != EventStatus.Removed);
            title = title?.Trim();

            if (!string.IsNullOrEmpty(title))
                events = events.Where(e => e.Title!.Contains(title));

            if (from != null)
                events = events.Where(e => e.StartAt >= from);

            if (to != null)
                events = events.Where(e => e.EndAt <= to);

            return events;
        }
    }
}
