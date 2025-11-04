using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class ProductViewConfiguration : IEntityTypeConfiguration<ProductView>
{
    public void Configure(EntityTypeBuilder<ProductView> builder)
    {
        builder.ToTable("ProductViews");

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.ProductId)
            .IsRequired();

        builder.Property(pv => pv.UserId)
            .HasMaxLength(100);

        builder.Property(pv => pv.IpAddress)
            .HasMaxLength(50);

        builder.Property(pv => pv.UserAgent)
            .HasMaxLength(500);

        builder.Property(pv => pv.ViewedAt)
            .IsRequired();

        builder.HasIndex(pv => new { pv.ProductId, pv.ViewedAt });
    }
}
