using YaEvents.Data.Models;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Infrastructure.Repositories.EventsRepository
{
    public class EventsRepository : IRepository<Event>
    {
        protected static int _lastId = 0;
        protected static Dictionary<int, Event> _events = new Dictionary<int, Event>();
        public Event Add(Event entity)
        {
            var newEvent = new Event
            {
                Id = ++_lastId,
                Description = entity.Description,
                EndAt = entity.EndAt,
                StartAt = entity.StartAt,
                Title = entity.Title
            };
            _events[newEvent.Id] = newEvent;

            return newEvent;
        }

        public void Change(Event entity)
        {
            _events[entity.Id] = entity;
        }

        public bool Delete(int id)
        {
            return _events.Remove(id);
        }

        public Event? Get(int id)
        {
            return _events.GetValueOrDefault(id);
        }

        public IEnumerable<Event> GetAll()
        {
            return _events.Values;
        }
    }
}
