using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        public Task<Message?> Get(Guid id, CancellationToken token = default);
        public Task Add(Message message, CancellationToken token = default);
    }
}
