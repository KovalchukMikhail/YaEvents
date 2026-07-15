using Application.DTO;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Repositories
{
    public interface IEventsRepository
    {
        public Task<Event> Add(Event curEvent, CancellationToken token = default);
        public Task<bool> Update(Guid id, CreateEvent createEvent, CancellationToken token = default);
        public Task<bool> Delete(Guid id, CancellationToken token = default);
        public Task<Event?> Get(Guid id, CancellationToken token = default);
        public Task<int> GetFilteredEventsCount(string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default);
        public Task<Event[]> GetFilteredEventsWithPagination(int pageNumber = 1, int pageSize = 10, string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default);
        public Task ReleaseSeats(Guid id, CancellationToken token = default);
        public Task<bool> TryReserveSeats(Guid id, CancellationToken token = default);
    }
}
