using System;
using System.Collections.Generic;
using System.Text;

namespace YaEventsConfigurations.Contracts
{
    public class BookingConfirmed
    {
        public Guid MessageId { get; set; }
        public Guid EventId { get; set; }
        public Guid BookingId { get; set; }
    }
}
