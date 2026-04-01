using System.Runtime.CompilerServices;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Models;
using YaEvents.Infrastructure.Repositories.EventsRepository;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Application.BackgroundServices
{
    public class BookingsBackgroundService : BackgroundService
    {
        private readonly ILogger<BookingsBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public BookingsBackgroundService(ILogger<BookingsBackgroundService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            _logger.LogInformation("BookingsBackgroundService запущен");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var scope = _scopeFactory.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    await bookingService.ProcessBookings(token);
                }
                catch(OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке бронирования");
                }

            }

            _logger.LogInformation("BookingsBackgroundService остановлен");
        }
    }
}
