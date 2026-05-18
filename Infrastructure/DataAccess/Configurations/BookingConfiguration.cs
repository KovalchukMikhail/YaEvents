using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaEvents.Data.Models;

namespace YaEvents.Infrastructure.DataAccess.Configurations
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

            builder.HasOne(b => b.Event)
                    .WithMany(e => e.Bookings)
                    .HasForeignKey(b => b.EventId);
        }
    }
}
