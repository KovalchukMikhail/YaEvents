using Microsoft.EntityFrameworkCore;
using YaEvents.Data.Models;

namespace YaEvents.Infrastructure.DataAccess
{
    public class AppDbContext : DbContext
    {
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Booking> Bookings => Set<Booking>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
