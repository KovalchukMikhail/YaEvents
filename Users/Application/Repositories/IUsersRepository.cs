using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Repositories
{
    public interface IUsersRepository
    {
        public Task<User> Add(User user, CancellationToken token = default);
        public Task<User?> Get(Guid Id, CancellationToken token = default);
        public Task<User?> Get(string login, CancellationToken token = default);
    }
}
