using Application.Repositories;
using Domain.Enums;
using Domain.Models;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories.BookingsRepository
{
    public class BookingsRepository : IBookingsRepository
    {
        protected readonly AppDbContext _appDbContext;
        public BookingsRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<Booking> Add(Booking booking, CancellationToken token = default)
        {
            await _appDbContext.Bookings.AddAsync(booking, token);
            await _appDbContext.SaveChangesAsync(token);
            return booking;
        }
        public async Task<Booking?> Get(Guid id, CancellationToken token = default)
        {
            return await _appDbContext.Bookings.Where(b => b.Id == id).Include(b => b.User).SingleAsync();
        }

        public async Task<Booking[]> GetPending(CancellationToken token = default)
        {
            return await _appDbContext.Bookings.Where(b => b.Status == BookingStatus.Pending).ToArrayAsync();
        }
        public async Task<bool> Confirm(Guid id, CancellationToken token = default)
        {
            var booking = await _appDbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null || !booking.Confirm())
                return false;

            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Cancel(Guid id, CancellationToken token = default)
        {
            var booking = await _appDbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null || !booking.Cancel())
                return false;

            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
