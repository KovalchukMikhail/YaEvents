using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using YaEventsConfigurations.Contracts;

namespace Infrastructure.Consumer.Interfaces
{
    public interface IEventsConsumer : IDisposable
    {
        public void ConsumeBookingConfirmed(Func<BookingConfirmed?, ConsumeResult<string, string>, Task> action);
        public void Close();
    }
}
