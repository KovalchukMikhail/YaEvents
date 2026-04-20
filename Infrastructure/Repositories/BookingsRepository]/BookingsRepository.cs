using System.Collections.Concurrent;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Infrastructure.Repositories.BookingsRepository
{
    public class BookingsRepository : IRepository<Booking>
    {
        protected static ConcurrentDictionary<Guid, Booking> _bookings = new ConcurrentDictionary<Guid, Booking>();
        public virtual async Task<Booking> Add(Booking booking, CancellationToken token = default)
        {
            _bookings.AddOrUpdate(booking.Id, booking, (id, b) => booking);

            return booking;
        }

        public virtual async Task Update(Booking booking, CancellationToken token = default)
        {
            _bookings.AddOrUpdate(booking.Id, booking, (id, b) => booking);
        }

        public virtual async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            return _bookings.Remove(id, out Booking? booking);
        }

        public virtual async Task<Booking?> Get(Guid id, CancellationToken token = default)
        {
            if(_bookings.TryGetValue(id, out Booking? booking))
            {
                return booking;
            }
            else
            {
                return null;
            }
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
