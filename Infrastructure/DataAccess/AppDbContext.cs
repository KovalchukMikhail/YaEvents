using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;


namespace Infrastructure.DataAccess
{
    public class AppDbContext : DbContext
    {
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Booking> Bookings => Set<Booking>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
