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

        builder.Property(b => b.YapePhone)
            .HasMaxLength(20);

        builder.Property(b => b.PlinPhone)
            .HasMaxLength(20);

        builder.Property(b => b.YapeQRUrl)
            .HasMaxLength(500);

        builder.Property(b => b.PlinQRUrl)
            .HasMaxLength(500);

        builder.Property(b => b.YapeEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.PlinEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.BankName)
            .HasMaxLength(100);

        builder.Property(b => b.BankAccountType)
            .HasMaxLength(20);

        builder.Property(b => b.BankAccountNumber)
            .HasMaxLength(50);

        builder.Property(b => b.BankCCI)
            .HasMaxLength(50);

        builder.Property(b => b.BankAccountVisible)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.DeliveryType)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Ambos");

        builder.Property(b => b.DeliveryCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.DeliveryZones)
            .HasMaxLength(1000);

        // Personalización de página principal
        builder.Property(b => b.HomeTitle)
            .HasMaxLength(200);
        builder.Property(b => b.HomeSubtitle)
            .HasMaxLength(500);
        builder.Property(b => b.HomeDescription)
            .HasMaxLength(1000);
        builder.Property(b => b.HomeBannerImageUrl)
            .HasMaxLength(500);

        // Solo debe haber un registro de BrandSettings
        builder.HasIndex(b => b.Id)
            .IsUnique();
    }
}
