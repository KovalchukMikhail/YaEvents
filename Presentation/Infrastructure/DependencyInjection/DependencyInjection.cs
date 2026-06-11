using Application.Repositories;
using Application.Services.BackgroundServices;
using Application.Services.BookingService;
using Application.Services.EventService;
using Application.Services.Interfaces;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.BookingsRepository;
using Infrastructure.Repositories.EventsRepository;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();

            services.AddHostedService<BookingsBackgroundService>();

            return services;
        }

        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddScoped<IEventsRepository, EventsRepository>();
            services.AddScoped<IBookingsRepository, BookingsRepository>();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
