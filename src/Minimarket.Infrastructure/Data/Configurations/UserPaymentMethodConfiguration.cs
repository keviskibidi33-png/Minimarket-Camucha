using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class UserPaymentMethodConfiguration : IEntityTypeConfiguration<UserPaymentMethod>
{
    public void Configure(EntityTypeBuilder<UserPaymentMethod> builder)
    {
        builder.ToTable("UserPaymentMethods");

        builder.HasKey(upm => upm.Id);

        builder.Property(upm => upm.UserId)
            .IsRequired();

        builder.Property(upm => upm.CardHolderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(upm => upm.CardNumberMasked)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(upm => upm.CardType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(upm => upm.ExpiryMonth)
            .IsRequired();

        builder.Property(upm => upm.ExpiryYear)
            .IsRequired();

        builder.Property(upm => upm.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(upm => upm.Last4Digits)
            .HasMaxLength(4);

        // Índice para UserId (un usuario puede tener múltiples métodos de pago)
        builder.HasIndex(upm => upm.UserId);
    }
}

