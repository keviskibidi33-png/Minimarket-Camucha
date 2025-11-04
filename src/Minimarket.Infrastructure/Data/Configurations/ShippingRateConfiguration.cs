using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class ShippingRateConfiguration : IEntityTypeConfiguration<ShippingRate>
{
    public void Configure(EntityTypeBuilder<ShippingRate> builder)
    {
        builder.ToTable("ShippingRates");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ZoneName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.BasePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.PricePerKm)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.PricePerKg)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.MinDistance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.MaxDistance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.MinWeight)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.MaxWeight)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.FreeShippingThreshold)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(s => s.ZoneName);
    }
}

