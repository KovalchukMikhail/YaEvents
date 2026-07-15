using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DataAccess.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.Property(p => p.Id)
                .IsRequired()
                .HasColumnName("id");

            builder.Property(p => p.Login)
                .HasColumnName("login")
                .IsRequired();

            builder.Property(p => p.Role)
                .HasColumnName("role")
                .IsRequired()
                .HasConversion<string>();

            builder.Property(p => p.PasswordHash)
                .IsRequired()
                .HasColumnName("password_hash");

            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.HasKey(p => p.Id);

            builder.HasIndex(p => p.Login)
                .IsUnique();
        }
    }
}
