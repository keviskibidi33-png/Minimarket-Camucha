using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;

namespace Minimarket.Infrastructure.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.DocumentNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.DocumentType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.SaleDate)
            .IsRequired();

        builder.Property(s => s.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.Tax)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.Discount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(s => s.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.PaymentMethod)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.AmountPaid)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.Change)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.CancellationReason)
            .HasMaxLength(500);

        // Propiedades de cierre de caja (requieren migración)
        // Si las columnas no existen, EF Core las ignorará automáticamente
        builder.Property(s => s.IsClosed)
            .HasDefaultValue(false);
        
        builder.Property(s => s.CashClosureDate);

        // Índice para IsClosed
        builder.HasIndex(s => s.IsClosed);

        // Relaciones
        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.SaleDetails)
            .WithOne(sd => sd.Sale)
            .HasForeignKey(sd => sd.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(s => s.DocumentNumber)
            .IsUnique();

        builder.HasIndex(s => s.SaleDate);
        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.UserId);
    }
}

