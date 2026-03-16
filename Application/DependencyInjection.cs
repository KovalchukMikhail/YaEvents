using YaEvents.Application.Services.EventService;
using YaEvents.Application.Services.Interfaces;

namespace YaEvents.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();

            return services;
        }
    }
}
