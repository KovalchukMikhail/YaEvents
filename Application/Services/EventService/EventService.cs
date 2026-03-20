using Microsoft.AspNetCore.Mvc.Diagnostics;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;
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
        public EventDto[] GetEvents(string? title = null, DateTime? from = null, DateTime? to = null)
        {
            IEnumerable<Event> events = _repository.GetAll();
            title = title?.Trim();

            if (!string.IsNullOrEmpty(title))
                events = events.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if(from != null)
                events = events.Where(e => e.StartAt >= from);

            if (to != null)
                events = events.Where(e => e.EndAt <= to);

            return events.Select(e => new EventDto(e.Id, e.Title, e.Description, e.StartAt, e.EndAt))
                         .ToArray();
        }
        public EventDto? GetEvent(int id)
        {
            var requiredEvent = _repository.Get(id);
            if (requiredEvent != null)
            {
                return new EventDto(requiredEvent.Id, requiredEvent.Title, requiredEvent.Description, requiredEvent.StartAt, requiredEvent.EndAt);
            }
            else
                return null;
        }

        public EventDto PostEvent(EventDtoLite eventDto)
        {
            var newEvent = new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartAt = eventDto.StartAt,
                EndAt = eventDto.EndAt
            };

            newEvent = _repository.Add(newEvent);

            return new EventDto(newEvent.Id, newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt);
        }

        public bool PutEvent(int id, EventDtoLite eventDto)
        {
            var requiredEvent = _repository.Get(id);
            if (requiredEvent != null)
            {
                requiredEvent.Title = eventDto.Title;
                requiredEvent.Description = eventDto.Description;
                requiredEvent.StartAt = eventDto.StartAt;
                requiredEvent.EndAt = eventDto.EndAt;

                _repository.Change(requiredEvent);

                return true;
            }
            else
                return false;
        }
        public bool DeleteEvent(int id)
        {
            return _repository.Delete(id);
        }
        public PaginatedResult<EventDto> GetEventsWithPagination(EventDto[] sourceEvents, int pageNumber, int pageSize)
        {
            var events = sourceEvents.Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToArray();

            int totalPages = (int)Math.Ceiling((double)sourceEvents.Length / pageSize);

            return new PaginatedResult<EventDto>(events, pageNumber, totalPages, events.Length, sourceEvents.Length);
        }
    }
}
