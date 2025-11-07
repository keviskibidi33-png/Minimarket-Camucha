using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");

        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.UserId)
            .IsRequired();

        builder.Property(ua => ua.Label)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ua => ua.IsDifferentRecipient)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ua => ua.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ua => ua.FirstName)
            .HasMaxLength(100);

        builder.Property(ua => ua.LastName)
            .HasMaxLength(100);

        builder.Property(ua => ua.Dni)
            .HasMaxLength(8);

        builder.Property(ua => ua.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ua => ua.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ua => ua.Reference)
            .HasMaxLength(500);

        builder.Property(ua => ua.District)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ua => ua.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ua => ua.Region)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ua => ua.PostalCode)
            .HasMaxLength(20);

        builder.Property(ua => ua.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(ua => ua.UserId);
    }
}

