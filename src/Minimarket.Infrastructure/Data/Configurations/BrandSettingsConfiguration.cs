using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class BrandSettingsConfiguration : IEntityTypeConfiguration<BrandSettings>
{
    public void Configure(EntityTypeBuilder<BrandSettings> builder)
    {
        builder.ToTable("BrandSettings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.LogoUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.StoreName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.FaviconUrl)
            .HasMaxLength(500);

        builder.Property(b => b.PrimaryColor)
            .IsRequired()
            .HasMaxLength(7); // #RRGGBB

        builder.Property(b => b.SecondaryColor)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(b => b.ButtonColor)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(b => b.TextColor)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(b => b.HoverColor)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        builder.Property(b => b.Slogan)
            .HasMaxLength(500);

        builder.Property(b => b.Phone)
            .HasMaxLength(20);

        builder.Property(b => b.Email)
            .HasMaxLength(100);

        builder.Property(b => b.Address)
            .HasMaxLength(500);

        builder.Property(b => b.Ruc)
            .HasMaxLength(20);

        // Solo debe haber un registro de BrandSettings
        builder.HasIndex(b => b.Id)
            .IsUnique();
    }
}

