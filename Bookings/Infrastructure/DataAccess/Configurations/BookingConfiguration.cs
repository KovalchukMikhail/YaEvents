using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DataAccess.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Status)
                    .IsRequired()
                    .HasConversion<string>();

            builder.Property(p => p.CreatedAt)
                    .IsRequired();

            builder.Property(p => p.UserId)
                    .HasColumnName("user_id");

            builder.Property(p => p.EventId)
                    .HasColumnName("event_id");

            builder.HasIndex(p => p.UserId);
            builder.HasIndex(p => p.EventId);
        }
    }
}
