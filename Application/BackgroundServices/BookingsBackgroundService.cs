using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Models;
using YaEvents.Infrastructure;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;
using YaEvents.Infrastructure.Repositories.EventsRepository;
using YaEvents.Infrastructure.Repositories.Interfaces;

namespace YaEvents.Application.BackgroundServices
{
    public class BookingsBackgroundService : BackgroundService
    {
        private readonly ILogger<BookingsBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);

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
                    List<Guid> pendingBookingIds;
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingsRepository>();
                        var pendingBooking = await bookingRepository.GetPending(token);
                        pendingBookingIds = pendingBooking.Select(b => b.Id).ToList();
                    }
                    var tasks = pendingBookingIds.Select(id =>
                        ProcessBookingAsync(id, token));

                    await Task.WhenAll(tasks);
                }
                catch(OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch(ValidationException validationException)
                {
                    _logger.LogWarning(validationException, "Ошибка при обработке бронирования");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке бронирования");
                }

            }

            _logger.LogInformation("BookingsBackgroundService остановлен");
        }

        public async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken = default)
        {
            try
            {
                await Task.Delay(ProcessingDelay, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingsRepository>();
                var eventRepository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();

                var booking = await bookingRepository.Get(bookingId, stoppingToken);
                if (booking == null || booking.Status != BookingStatus.Pending)
                    return;

                _logger.LogInformation("Обрабатывается бронирование Id = {id}", booking.Id);

                var @event = await eventRepository.Get(booking.EventId, stoppingToken);
                if (@event == null)
                {
                    await RejectBookingAsync(booking, @event, eventRepository, bookingRepository, stoppingToken);
                    throw new ValidationException("Не удалось обработать объект бронирования так как объект события отсутствует") { EntityId = booking.Id };
                }
                else if (@event.Status == EventStatus.Removed)
                {
                    await RejectBookingAsync(booking, @event, eventRepository, bookingRepository, stoppingToken);   
                    throw new ValidationException("Не удалось обработать объект бронирования так как объект события помечен как удаленный") { EntityId = booking.Id };
                }

                var bookingSemaphore = AppSemaphores.GetSemaphore(booking.Id);
                await bookingSemaphore.WaitAsync();
                try
                {
                    await bookingRepository.Confirm(booking.Id, stoppingToken);
                }
                finally
                {
                    bookingSemaphore.Release();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (ValidationException validationException)
            {
                _logger.LogWarning(validationException, "Ошибка при обработке бронирования");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке бронирования");
            }
        }

        public async Task RejectBookingAsync(Booking booking, Event? curEvent, IEventsRepository eventsRepository, IBookingsRepository bookingsRepository, CancellationToken token = default)
        {
            if (curEvent == null || curEvent.Id != booking.EventId)
            {
                curEvent = await eventsRepository.Get(booking.EventId, token);
            }

            var bookingSemaphore = AppSemaphores.GetSemaphore(booking.Id);
            await bookingSemaphore.WaitAsync(token);
            try
            {
                if (await bookingsRepository.Reject(booking.Id, token))
                {
                    if (curEvent != null)
                    {
                        var eventSemaphore = AppSemaphores.GetSemaphore(curEvent.Id);
                        await eventSemaphore.WaitAsync(token);
                        try
                        {
                            await eventsRepository.ReleaseSeats(curEvent.Id, token);
                        }
                        finally
                        {
                            eventSemaphore.Release();
                        }
                    }
                }
            }
            finally
            {
                bookingSemaphore.Release();
            }
        }
    }
}
