using Application.Cache;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using YaEventsConfigurations;

namespace Infrastructure.CasheRepositories
{
    public class RedisCasheRepository : ICasheRepository
    {
        private IConnectionMultiplexer _redis;
        private ILogger<RedisCasheRepository> _logger;
        private IConfiguration _configuration;
        public RedisCasheRepository(IConnectionMultiplexer redis, ILogger<RedisCasheRepository> logger, IConfiguration configuration)
        {
            _redis = redis;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task AddEventToCashe(Event @event)
        {
            var redisDb = await GetDatabase();
            if (redisDb == null)
                return;

            if (!int.TryParse(_configuration["Redis:TTL:Event"], out int ttl))
            {
                ttl = 1;
            }

            await redisDb.StringSetAsync($"{RedisConfigurations.EVENT_REDIS_KEY}{@event.Id}", JsonSerializer.Serialize(@event), TimeSpan.FromMinutes(ttl));
        }
        public async Task<Event?> GetEvent(Guid id)
        {
            var redisDb = await GetDatabase();
            if (redisDb == null)
                return null;

            var @event = await redisDb.StringGetAsync($"{RedisConfigurations.EVENT_REDIS_KEY}{id}");
            if (@event.HasValue)
            {
                return JsonSerializer.Deserialize<Event>(@event.ToString());
            }

            return null;
        }
        public async Task AddTopTenEventsToCashe(Event[] events)
        {
            var redisDb = await GetDatabase();
            if (redisDb == null)
                return;
            
            if(!int.TryParse(_configuration["Redis:TTL:TopTenEvents"], out int ttl))
            {
                ttl = 60;
            }

            await redisDb.StringSetAsync(RedisConfigurations.TOP_TEN_EVENTS_REDIS_KEY, JsonSerializer.Serialize(events), TimeSpan.FromMinutes(ttl));
        }
        public async Task<Event[]?> GetTopTenEvents()
        {
            var redisDb = await GetDatabase();
            if (redisDb == null)
                return null;

            var events = await redisDb.StringGetAsync(RedisConfigurations.TOP_TEN_EVENTS_REDIS_KEY);
            if (events.HasValue)
            {
                return JsonSerializer.Deserialize<Event[]>(events.ToString());
            }

            return null;
        }

        public async Task RemoveEventFromCache(Guid id)
        {
            var redisDb = await GetDatabase();
            if (redisDb == null)
                return;

            await redisDb.KeyDeleteAsync($"{RedisConfigurations.EVENT_REDIS_KEY}{id}");
        }

        private async Task<IDatabase?> GetDatabase()
        {
            IDatabase? db = null;
            try
            {
                db = _redis.GetDatabase();
                await db.PingAsync();
            }
            catch(RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Отсутсвует соединение с Redis");
                return null;
            }

            return db;
        }
    }
}
