using YaEvents.Data.Models;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Infrastructure.Repositories.BookingsRepository
{
    public class BookingsRepository : IRepository<Booking>
    {
        protected static Dictionary<Guid, Booking> _bookings = new Dictionary<Guid, Booking>();
        public virtual async Task<Booking> Add(Booking booking, CancellationToken token = default)
        {
            _bookings[booking.Id] = booking;

            return booking;
        }

        public virtual async Task Update(Booking booking, CancellationToken token = default)
        {
            _bookings[booking.Id] = booking;
        }

        public virtual async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            return _bookings.Remove(id);
        }

        public virtual async Task<Booking?> Get(Guid id, CancellationToken token = default)
        {
            return _bookings.GetValueOrDefault(id);
        }

        public virtual async Task<IEnumerable<Booking>> GetAll(CancellationToken token = default)
        {
            return _bookings.Values;
        }

        public virtual async Task<IEnumerable<Booking>> GetPending(CancellationToken token = default)
        {
            return _bookings.Values.Where(b => b.Status == Enums.BookingStatus.Pending);
        }
    }
}
