using YaEvents.Data.Models;

namespace YaEvents.Infrastructure.Repositories.Interfaces
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
