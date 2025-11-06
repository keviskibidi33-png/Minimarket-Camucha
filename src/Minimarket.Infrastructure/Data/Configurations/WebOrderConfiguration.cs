using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class WebOrderConfiguration : IEntityTypeConfiguration<WebOrder>
{
    public void Configure(EntityTypeBuilder<WebOrder> builder)
    {
        builder.ToTable("WebOrders");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.CustomerEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(w => w.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.CustomerPhone)
            .HasMaxLength(20);

        builder.Property(w => w.ShippingMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.ShippingAddress)
            .HasMaxLength(500);

        builder.Property(w => w.ShippingCity)
            .HasMaxLength(100);

        builder.Property(w => w.ShippingRegion)
            .HasMaxLength(100);

        builder.Property(w => w.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.WalletMethod)
            .HasMaxLength(50);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("pending");

        builder.Property(w => w.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(w => w.ShippingCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(w => w.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(w => w.TrackingUrl)
            .HasMaxLength(500);

        // Relación con Sede
        builder.HasOne(w => w.SelectedSede)
            .WithMany()
            .HasForeignKey(w => w.SelectedSedeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relación con OrderItems
        builder.HasMany(w => w.OrderItems)
            .WithOne(oi => oi.WebOrder)
            .HasForeignKey(oi => oi.WebOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.OrderNumber);
        builder.HasIndex(w => w.CustomerEmail);
        builder.HasIndex(w => w.Status);
    }
}

