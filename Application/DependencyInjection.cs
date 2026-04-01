using YaEvents.Application.BackgroundServices;
using YaEvents.Application.Services.BookingService;
using YaEvents.Application.Services.EventService;
using YaEvents.Application.Services.Interfaces;

namespace YaEvents.Application
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
    }
}
