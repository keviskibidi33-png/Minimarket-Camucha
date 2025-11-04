using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class PageViewConfiguration : IEntityTypeConfiguration<PageView>
{
    public void Configure(EntityTypeBuilder<PageView> builder)
    {
        builder.ToTable("PageViews");

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.PageSlug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pv => pv.UserId)
            .HasMaxLength(100);

        builder.Property(pv => pv.IpAddress)
            .HasMaxLength(50);

        builder.Property(pv => pv.UserAgent)
            .HasMaxLength(500);

        builder.Property(pv => pv.ViewedAt)
            .IsRequired();

        builder.HasIndex(pv => new { pv.PageSlug, pv.ViewedAt });
    }
}
