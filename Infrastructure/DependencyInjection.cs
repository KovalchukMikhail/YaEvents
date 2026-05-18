using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using YaEvents.Application.Services.EventService;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Repositories.BookingsRepository;
using YaEvents.Infrastructure.Repositories.EventsRepository;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddScoped<IRepository<Event>, EventsRepository>();
            services.AddScoped<BookingsRepository>();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
