using Application.Repositories;
using Confluent.Kafka;
using Infrastructure.Consumer.Interfaces;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using YaEventsConfigurations.Contracts;
using static Confluent.Kafka.ConfigPropertyNames;

namespace Infrastructure.Consumer
{
    public class EventsConsumer : IEventsConsumer
    {
        private readonly IConsumer<string, string> _consumer;

        public EventsConsumer(IConfiguration configuration)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = configuration.GetSection("Kafka")["BootstrapServers"],
                GroupId = configuration.GetSection("Kafka")["ConsumerGroup"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe(YaEventsConfigurations.KafkaConfigurations.CREATING_BOOKINGS_TOPIC_NAME);

        }
        public async void ConsumeBookingConfirmed(Func<BookingConfirmed?, ConsumeResult<string, string>, Task> action)
        {
            var consumeResult = _consumer.Consume();
            var bookingConfirmed = JsonSerializer.Deserialize<BookingConfirmed>(consumeResult.Message.Value);

            await action(bookingConfirmed, consumeResult);

            _consumer.StoreOffset(consumeResult);
            _consumer.Commit(consumeResult);
        }
        public void Close()
        {
            if(_consumer != null)
            {
                _consumer.Close();
            }
        }
        public void Dispose()
        {
            if(_consumer != null)
            {
                _consumer.Dispose();
            }
        }
    }
}
