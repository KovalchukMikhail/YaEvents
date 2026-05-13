using System.Collections.Concurrent;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Infrastructure.Repositories.EventsRepository
{
    public class EventsRepository : IRepository<Event>
    {
        protected static ConcurrentDictionary<Guid, Event> _events = new ConcurrentDictionary<Guid, Event>();
        //protected static Dictionary<Guid, Event> _events = new Dictionary<Guid, Event>();
        public async Task<Event> Add(Event entity, CancellationToken token = default)
        {
            _events.AddOrUpdate(entity.Id, entity, (id, e) => entity);

            return entity;
        }

        public async Task Update(Event entity, CancellationToken token = default)
        {
            _events.AddOrUpdate(entity.Id, entity, (id, e) => entity);
        }

        public async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            if(_events.TryGetValue(id, out var curEvent))
            {
                if(curEvent.Status == Enums.EventStatus.Removed)
                    return false;
                else
                {
                    curEvent.Status = Enums.EventStatus.Removed;
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
            if (_events.TryGetValue(id, out var curEvent))
            {
                return curEvent;
            }
            else
                return null;
        }

        public async Task<IEnumerable<Event>> GetAll(CancellationToken token = default)
        {
            return _events.Values;
        }
    }
}
