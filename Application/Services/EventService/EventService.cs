using Microsoft.AspNetCore.Mvc.Diagnostics;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Repositories.Interfaces;
using YaEvents.Infrastructure.Exceptions;

namespace YaEvents.Application.Services.EventService
{
    public class EventService : IEventService
    {
        protected readonly IRepository<Event> _repository;
        public EventService(IRepository<Event> repository)
        {
            _repository = repository;
        }
        public async Task<EventInfo[]> GetEvents(string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default)
        {
            IEnumerable<Event> events = await _repository.GetAll(token: token);
            title = title?.Trim();

            if (!string.IsNullOrEmpty(title))
                events = events.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if(from != null)
                events = events.Where(e => e.StartAt >= from);

            if (to != null)
                events = events.Where(e => e.EndAt <= to);

            return events.Select(e => new EventInfo(e.Id, e.Title, e.Description, e.StartAt, e.EndAt, e.Status, e.TotalSeats, e.AvailableSeats))
                         .ToArray();
        }
        public async Task<EventInfo?> GetEvent(Guid id, CancellationToken token = default)
        {
            var requiredEvent = await _repository.Get(id, token: token);
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
            {
                Id = Guid.NewGuid(),
                Title = createEvent.Title,
                Description = createEvent.Description,
                StartAt = createEvent.StartAt,
                EndAt = createEvent.EndAt,
                Status = EventStatus.Existing,
                TotalSeats = createEvent.TotalSeats,
                AvailableSeats = createEvent.TotalSeats

            };

            await _repository.Add(newEvent, token: token);

            return new EventInfo(newEvent.Id, newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt, newEvent.Status, newEvent.TotalSeats, newEvent.AvailableSeats);
        }

        public async Task<bool> PutEvent(Guid id, CreateEvent createEvent, CancellationToken token = default)
        {
            var requiredEvent = await _repository.Get(id, token: token);
            if (requiredEvent != null && requiredEvent.Status == EventStatus.Existing)
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

                await _repository.Update(requiredEvent, token: token);

                return true;
            }
            else
                return false;
        }
        public async Task<bool> DeleteEvent(Guid id, CancellationToken token = default)
        {
            return await _repository.Delete(id, token: token);
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
