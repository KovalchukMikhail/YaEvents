using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class Message
    {
        public Guid Id { get; init; }
        private Message() { }
        public Message(Guid id)
        {
            Id = id;
        }
    }
}
