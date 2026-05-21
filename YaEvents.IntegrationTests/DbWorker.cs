using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;
using YaEvents.Infrastructure.DataAccess;

namespace YaEvents.IntegrationTests
{
    public class DbWorker : IAsyncLifetime
    {
        private static readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }
        public async Task<AppDbContext> CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                                .UseNpgsql(_postgres.GetConnectionString())
                                .Options;

            var context = new AppDbContext(options);
            context.Database.Migrate();

            return context;
        }

        public async Task ResetDatabaseAsync()
        {
            await using var context = await CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE bookings, events RESTART IDENTITY CASCADE");
        }
    }
}
