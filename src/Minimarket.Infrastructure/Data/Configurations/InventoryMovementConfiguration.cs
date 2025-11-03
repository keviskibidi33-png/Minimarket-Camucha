using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;

namespace Minimarket.Infrastructure.Data.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements");

        builder.HasKey(im => im.Id);

        builder.Property(im => im.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(im => im.Quantity)
            .IsRequired();

        builder.Property(im => im.Reason)
            .HasMaxLength(500);

        builder.Property(im => im.Reference)
            .HasMaxLength(100);

        builder.Property(im => im.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(im => im.Notes)
            .HasMaxLength(1000);

        // Relaciones
        builder.HasOne(im => im.Product)
            .WithMany()
            .HasForeignKey(im => im.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(im => im.Sale)
            .WithMany()
            .HasForeignKey(im => im.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(im => im.ProductId);
        builder.HasIndex(im => im.Type);
        builder.HasIndex(im => im.CreatedAt);
        builder.HasIndex(im => im.SaleId);
    }
}

