using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTO
{
    public class BookingDtoLite
    {
        [Required(ErrorMessage = "Идентификатор события обязателен для заполнения.")]
        public Guid EventId { get; set; }
        public required BookingStatus Status { get; set; }
        public required DateTime CreatedAt { get; init; }
        public DateTime? ProcessedAt { get; set; }
    }
}
