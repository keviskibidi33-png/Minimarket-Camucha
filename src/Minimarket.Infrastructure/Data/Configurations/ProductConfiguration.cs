using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.PurchasePrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.SalePrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Stock)
            .IsRequired();

        builder.Property(p => p.MinimumStock)
            .IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        builder.Property(p => p.ImagenesJson)
            .IsRequired()
            .HasMaxLength(2000)
            .HasDefaultValue("[]");

        builder.Property(p => p.PaginasJson)
            .IsRequired()
            .HasMaxLength(500)
            .HasDefaultValue("{}");

        builder.Property(p => p.SedesDisponiblesJson)
            .IsRequired()
            .HasMaxLength(1000)
            .HasDefaultValue("[]");

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

