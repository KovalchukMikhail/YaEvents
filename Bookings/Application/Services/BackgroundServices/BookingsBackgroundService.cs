using Application.Repositories;
using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Domain.Exceptions;
using Application.Producer;

namespace Application.Services.BackgroundServices
{
    public class BookingsBackgroundService : BackgroundService
    {
        private readonly ILogger<BookingsBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IBookingsProducer _bookingsProducer;

        private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);

        public BookingsBackgroundService(ILogger<BookingsBackgroundService> logger, IServiceScopeFactory scopeFactory, IBookingsProducer bookingsProducer)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _bookingsProducer = bookingsProducer;
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

                    await Task.Delay(ProcessingDelay);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (DomainValidationException validationException)
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
                using var scope = _scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingsRepository>();
            
                var booking = await bookingRepository.Get(bookingId, stoppingToken);
                if (booking == null || booking.Status != BookingStatus.Pending)
                    return;
            
                _logger.LogInformation("Обрабатывается бронирование Id = {id}", booking.Id);
            
                await bookingRepository.Confirm(booking.Id, stoppingToken);

                await _bookingsProducer.ProduceBookingConfirmedAsync(booking.Id, booking.EventId, booking.UserId, 1, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке бронирования");
            }
        }
    }
}
