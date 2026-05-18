using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaEvents.Data.Models;

namespace YaEvents.Infrastructure.DataAccess.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("events");

            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title).IsRequired();

            builder.Property(p => p.Description).IsRequired();
        }
    }
}
