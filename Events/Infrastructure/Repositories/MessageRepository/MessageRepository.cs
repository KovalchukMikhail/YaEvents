using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories.MessageRepository
{
    public class MessageRepository : IMessageRepository
    {
        public AppDbContext _appDbContext;
        public MessageRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<Message?> Get(Guid id, CancellationToken token = default)
        {
            return await _appDbContext.Messages.Where(m => m.Id == id).FirstOrDefaultAsync();
        }
        public async Task Add(Message message, CancellationToken token = default)
        {
            await _appDbContext.Messages.AddAsync(message, token);
            await _appDbContext.SaveChangesAsync(token);
        }
    }
}
