using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Repositories
{
    public interface IBookingsRepository
    {
        public Task<Booking> Add(Booking booking, CancellationToken token = default);
        public Task<Booking?> Get(Guid id, CancellationToken token = default);
        public Task<Booking[]> GetPending(CancellationToken token = default);
        public Task<bool> Confirm(Guid id, CancellationToken token = default);
        public Task<bool> Reject(Guid id, CancellationToken token = default);
    }
}
