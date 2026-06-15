using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class User
    {
        public Guid Id { get; init; }
        public string? Login { get; set; }
        public string? PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public List<Booking>? Bookings { get; set; }

        private User() { }

        public User(Guid id, string login, string passwordHash, UserRole role)
        {
            Id = id;
            Login = login;
            PasswordHash = passwordHash;
            Role = role;
        }

    }
}
