using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Cache
{
    public interface ICasheRepository
    {
        public Task<Event?> GetEvent(Guid id);
        public Task<Event[]?> GetTopTenEvents();
        public Task AddEventToCashe(Event @event);
        public Task AddTopTenEventsToCashe(Event[] events);
        public Task RemoveEventFromCache(Guid id);
    }
}
