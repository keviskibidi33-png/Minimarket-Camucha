using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class PaymentMethodSettingsConfiguration : IEntityTypeConfiguration<PaymentMethodSettings>
{
    public void Configure(EntityTypeBuilder<PaymentMethodSettings> builder)
    {
        builder.ToTable("PaymentMethodSettings");

        builder.HasKey(pms => pms.Id);

        builder.Property(pms => pms.PaymentMethodId)
            .IsRequired();

        builder.Property(pms => pms.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pms => pms.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pms => pms.RequiresCardDetails)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pms => pms.Description)
            .HasMaxLength(500);

        builder.Property(pms => pms.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Índice único para PaymentMethodId
        builder.HasIndex(pms => pms.PaymentMethodId)
            .IsUnique();
    }
}

