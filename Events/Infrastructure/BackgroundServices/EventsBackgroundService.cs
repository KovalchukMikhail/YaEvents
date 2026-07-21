using Application.Cache;
using Application.Repositories;
using Confluent.Kafka;
using Infrastructure.Consumer.Interfaces;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using YaEventsConfigurations.Contracts;

namespace Infrastructure.BackgroundServices
{
    public class EventsBackgroundService : BackgroundService
    {
        private readonly ILogger<EventsBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEventsConsumer _eventsConsumer;
        private readonly TopicBootstrapper _topicBootstrapper;

        public EventsBackgroundService(ILogger<EventsBackgroundService> logger, IServiceScopeFactory scopeFactory, IEventsConsumer eventsConsumer, TopicBootstrapper topicBootstrapper)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _eventsConsumer = eventsConsumer;
            _topicBootstrapper = topicBootstrapper;
        }

        protected override Task ExecuteAsync(CancellationToken token = default)
        {
            return Task.Run(() => Consume(token), token);
        }
        private async Task Consume(CancellationToken token)
        {
            _logger.LogInformation("EventsBackgroundService запущен");
            await _topicBootstrapper.StartAsync(token);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await _eventsConsumer.ConsumeBookingConfirmed(ProcessBookingConfirmedMessage);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("IEventsConsumer остановлен штатно.");
            }
            finally
            {
                _eventsConsumer.Close();
            }

            _logger.LogInformation("EventsBackgroundService остановлен");
        }
        private async Task ProcessBookingConfirmedMessage(BookingConfirmed? bookingConfirmed, ConsumeResult<string, string> consumeResult)
        {
            if (bookingConfirmed == null)
            {
                _logger.LogWarning("Неудалось десериализовать сообщение Topic={Topic}, Partition={Partition}, Offset={Offset}",
                    consumeResult.Topic,
                    consumeResult.Partition,
                    consumeResult.Offset);

                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
                var message = await messageRepository.Get(bookingConfirmed.MessageId);
                if(message != null)
                {
                    _logger.LogInformation("Сообщение уже обработано. MessageId={MessageId}, BookingId={BookingId}, EventId={EventId}",
                        bookingConfirmed.MessageId,
                        bookingConfirmed.BookingId,
                        bookingConfirmed.EventId);
                    return;
                }

                var eventsRepository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
                var @event = await eventsRepository.Get(bookingConfirmed.EventId);
                if(@event == null)
                {
                    _logger.LogWarning("Не удалось подтвердить бронирование. Событие переданное в сообщении не существует. MessageId={MessageId}, BookingId={BookingId}, EventId={EventId}",
                        bookingConfirmed.MessageId,
                        bookingConfirmed.BookingId,
                        bookingConfirmed.EventId);

                    return;
                }

                if(@event.AvailableSeats == 0)
                {
                    _logger.LogInformation("Неудалось подтвердить бронирование. Отсутсвуют свободные места. MessageId={MessageId}, BookingId={BookingId}, EventId={EventId}",
                        bookingConfirmed.MessageId,
                        bookingConfirmed.BookingId,
                        bookingConfirmed.EventId);

                    return;
                }

                var casheRepository = scope.ServiceProvider.GetRequiredService<ICasheRepository>();
                await eventsRepository.TryReserveSeats(@event.Id);
                await casheRepository.RemoveEventFromCache(@event.Id);
                message = new Domain.Models.Message(bookingConfirmed.MessageId);
                await messageRepository.Add(message);
            }
        }

        //public async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken = default)
        //{
        //    try
        //    {
        //        await Task.Delay(ProcessingDelay, stoppingToken);
        //
        //        using var scope = _scopeFactory.CreateScope();
        //        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingsRepository>();
        //
        //        var booking = await bookingRepository.Get(bookingId, stoppingToken);
        //        if (booking == null || booking.Status != BookingStatus.Pending)
        //            return;
        //
        //        _logger.LogInformation("Обрабатывается бронирование Id = {id}", booking.Id);
        //
        //        await bookingRepository.Confirm(booking.Id, stoppingToken);
        //
        //        await _bookingsProducer.ProduceCreatedBookingAsync(booking.Id, booking.EventId, stoppingToken);
        //    }
        //    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Ошибка при обработке бронирования");
        //    }
        //}
    }
}
