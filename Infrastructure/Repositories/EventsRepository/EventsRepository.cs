using YaEvents.Data.Models;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Infrastructure.Repositories.EventsRepository
{
    public class EventsRepository : IRepository<Event>
    {
        protected static Dictionary<Guid, Event> _events = new Dictionary<Guid, Event>();
        public async Task<Event> Add(Event entity, CancellationToken token = default)
        {
            _events[entity.Id] = entity;

            return entity;
        }

        public async Task Change(Event entity, CancellationToken token = default)
        {
            _events[entity.Id] = entity;
        }

        public async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            if (_events[id].Status == Enums.EventStatus.Removed)
                return false;
            else
            {
                _events[id].Status = Enums.EventStatus.Removed;
                return true;
            }
        }

        public async Task<Event?> Get(Guid id, CancellationToken token = default)
        {
            return _events.GetValueOrDefault(id);
        }

        public async Task<IEnumerable<Event>> GetAll(CancellationToken token = default)
        {
            return _events.Values;
        }
    }
}
