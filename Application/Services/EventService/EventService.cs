using Microsoft.AspNetCore.Mvc.Diagnostics;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Data.Models;

namespace YaEvents.Application.Services.EventService
{
    public class EventService : IEventService
    {
        protected static int _lastId = 0;
        protected static Dictionary<int, Event> _events = new Dictionary<int, Event>();
        public EventDto[] GetAllEvents()
        {
            return _events.Values.OrderBy(e => e.Id)
                                        .Select(e =>
                                        {
                                            return new EventDto
                                            {
                                                Id = e.Id,
                                                Title = e.Title,
                                                Description = e.Description,
                                                StartAt = e.StartAt,
                                                EndAt = e.EndAt,
                                            };
                                        }).ToArray();
        }

        public EventDto GetEvent(int id)
        {
            if (_events.TryGetValue(id, out Event requiredEvent))
            {
                return new EventDto
                {
                    Id = requiredEvent.Id,
                    Title = requiredEvent.Title,
                    Description = requiredEvent.Description,
                    StartAt = requiredEvent.StartAt,
                    EndAt = requiredEvent.EndAt,
                };
            }
            else
                return null;
        }

        public EventDto PostEvent(EventDtoLite eventDto)
        {
            var newEvent = new Event
            {
                Id = ++_lastId,
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartAt = eventDto.StartAt,
                EndAt = eventDto.EndAt
            };

            _events[newEvent.Id] = newEvent;

            return new EventDto
                { Id = newEvent.Id,
                    Title = newEvent.Title,
                    Description = newEvent.Description,
                    StartAt = newEvent.StartAt,
                    EndAt = newEvent.EndAt 
                };
        }

        public bool PutEvent(int id, EventDtoLite eventDto)
        {
            if (_events.TryGetValue(id, out Event requiredEvent))
            {
                requiredEvent.Title = eventDto.Title;
                requiredEvent.Description = eventDto.Description;
                requiredEvent.StartAt = eventDto.StartAt;
                requiredEvent.EndAt = eventDto.EndAt;

                return true;
            }
            else
                return false;
        }
        public bool DeleteEvent(int id)
        {
            return _events.Remove(id);
        }
    }
}
