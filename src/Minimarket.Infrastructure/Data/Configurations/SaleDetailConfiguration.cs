using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.ToTable("SaleDetails");

        builder.HasKey(sd => sd.Id);

        builder.Property(sd => sd.Quantity)
            .IsRequired();

        builder.Property(sd => sd.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sd => sd.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(sd => sd.Product)
            .WithMany(p => p.SaleDetails)
            .HasForeignKey(sd => sd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

