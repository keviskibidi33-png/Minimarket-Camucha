using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("Banners");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.Property(b => b.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.LinkUrl)
            .HasMaxLength(500);

        builder.Property(b => b.Position)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(b => b.Position);
        builder.HasIndex(b => b.DisplayOrder);
        builder.HasIndex(b => b.IsActive);
    }
}

