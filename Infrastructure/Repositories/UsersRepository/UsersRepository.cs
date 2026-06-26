using Application.Repositories;
using Domain.Models;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories.UsersRepository
{
    public class UsersRepository : IUsersRepository
    {
        protected readonly AppDbContext _appDbContext;
        public UsersRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<User> Add(User user, CancellationToken token = default)
        {
            _appDbContext.Users.Add(user);
            await _appDbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User?> Get(Guid id, CancellationToken token = default)
        {
            return await _appDbContext.Users.Where(u => u.Id == id).Include(u => u.Bookings)!.ThenInclude(b => b.Event).SingleAsync(token);
        }

        public async Task<User?> Get(string login, CancellationToken token = default)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
        }
    }
}
