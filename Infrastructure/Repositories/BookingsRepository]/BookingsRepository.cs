using YaEvents.Data.Models;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Infrastructure.Repositories.BookingsRepository
{
    public class BookingsRepository : IRepository<Booking>
    {
        protected static Dictionary<Guid, Booking> _bookings = new Dictionary<Guid, Booking>();
        public async Task<Booking> Add(Booking booking, CancellationToken token = default)
        {
            _bookings[booking.Id] = booking;

            return booking;
        }

        public async Task Change(Booking booking, CancellationToken token = default)
        {
            _bookings[booking.Id] = booking;
        }

        public async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            return _bookings.Remove(id);
        }

        public async Task<Booking?> Get(Guid id, CancellationToken token = default)
        {
            return _bookings.GetValueOrDefault(id);
        }

        public async Task<IEnumerable<Booking>> GetAll(CancellationToken token = default)
        {
            return _bookings.Values;
        }
    }
}
