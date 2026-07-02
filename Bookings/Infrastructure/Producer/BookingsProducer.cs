using Application.Producer;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using YaEventsConfigurations.Contracts;

namespace Infrastructure.Producer
{
    public class BookingsProducer : IBookingsProducer
    {
        public IProducer<string, string> _producer;
        public BookingsProducer(IConfiguration configuration)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration.GetSection("Kafka")["BootstrapServers"],
                Acks = Acks.All
            };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceBookingConfirmedAsync(Guid bookingId, Guid eventId, CancellationToken token = default)
        {
            var creatingBooking = new BookingConfirmed
            {
                MessageId = Guid.NewGuid(),
                BookingId = bookingId,
                EventId = eventId
            };

            await _producer.ProduceAsync(YaEventsConfigurations.KafkaConfigurations.CREATING_BOOKINGS_TOPIC_NAME, new Message<string, string>
            {
                Key = creatingBooking.EventId.ToString(),
                Value = JsonSerializer.Serialize(creatingBooking)
            });

        }
        public void Dispose()
        {
            if(_producer != null)
            {
                _producer.Dispose();
            }
        }
    }
}
