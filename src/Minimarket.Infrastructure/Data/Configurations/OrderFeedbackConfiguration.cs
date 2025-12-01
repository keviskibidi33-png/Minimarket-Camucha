using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class OrderFeedbackConfiguration : IEntityTypeConfiguration<OrderFeedback>
{
    public void Configure(EntityTypeBuilder<OrderFeedback> builder)
    {
        builder.ToTable("OrderFeedbacks");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Rating)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(f => f.Comment)
            .HasMaxLength(1000);

        builder.Property(f => f.WouldRecommend)
            .IsRequired()
            .HasDefaultValue(true);

        // Relación con WebOrder
        builder.HasOne(f => f.WebOrder)
            .WithMany()
            .HasForeignKey(f => f.WebOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice para búsquedas por pedido
        builder.HasIndex(f => f.WebOrderId);
    }
}

