using Microsoft.AspNetCore.Mvc.Diagnostics;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Application.Services.EventService
{
    public class EventService : IEventService
    {
        protected readonly IRepository<Event> _repository;
        public EventService(IRepository<Event> repository)
        {
            _repository = repository;
        }
        public async Task<EventDto[]> GetEvents(string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default)
        {
            IEnumerable<Event> events = await _repository.GetAll(token: token);
            title = title?.Trim();

            if (!string.IsNullOrEmpty(title))
                events = events.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if(from != null)
                events = events.Where(e => e.StartAt >= from);

            if (to != null)
                events = events.Where(e => e.EndAt <= to);

            return events.Select(e => new EventDto(e.Id, e.Title, e.Description, e.StartAt, e.EndAt, e.Status))
                         .ToArray();
        }
        public async Task<EventDto?> GetEvent(Guid id, CancellationToken token = default)
        {
            var requiredEvent = await _repository.Get(id, token: token);
            if (requiredEvent != null)
            {
                return new EventDto(requiredEvent.Id, requiredEvent.Title, requiredEvent.Description, requiredEvent.StartAt, requiredEvent.EndAt, requiredEvent.Status);
            }
            else
                return null;
        }

        public async Task<EventDto> PostEvent(EventDtoLite eventDto, CancellationToken token = default)
        {
            var newEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartAt = eventDto.StartAt,
                EndAt = eventDto.EndAt,
                Status = EventStatus.Existing
            };

            await _repository.Add(newEvent, token: token);

            return new EventDto(newEvent.Id, newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt, newEvent.Status);
        }

        public async Task<bool> PutEvent(Guid id, EventDtoLite eventDto, CancellationToken token = default)
        {
            var requiredEvent = await _repository.Get(id, token: token);
            if (requiredEvent != null && requiredEvent.Status == EventStatus.Existing)
            {
                requiredEvent.Title = eventDto.Title;
                requiredEvent.Description = eventDto.Description;
                requiredEvent.StartAt = eventDto.StartAt;
                requiredEvent.EndAt = eventDto.EndAt;

                await _repository.Change(requiredEvent, token: token);

                return true;
            }
            else
                return false;
        }
        public async Task<bool> DeleteEvent(Guid id, CancellationToken token = default)
        {
            return await _repository.Delete(id, token: token);
        }
        public async Task<PaginatedResult<EventDto>> GetEventsWithPagination(EventDto[] sourceEvents, int pageNumber, int pageSize, CancellationToken token = default)
        {
            var events = sourceEvents.Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToArray();

            int totalPages = (int)Math.Ceiling((double)sourceEvents.Length / pageSize);

            return new PaginatedResult<EventDto>(events, pageNumber, totalPages, events.Length, sourceEvents.Length);
        }
    }
}
