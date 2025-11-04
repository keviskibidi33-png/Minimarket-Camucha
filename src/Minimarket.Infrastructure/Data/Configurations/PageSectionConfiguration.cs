using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class PageSectionConfiguration : IEntityTypeConfiguration<PageSection>
{
    public void Configure(EntityTypeBuilder<PageSection> builder)
    {
        builder.ToTable("PageSections");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.PageId)
            .IsRequired();

        builder.Property(ps => ps.SeccionTipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ps => ps.Orden)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ps => ps.DatosJson)
            .IsRequired()
            .HasMaxLength(5000)
            .HasDefaultValue("{}");

        // Relación con Page
        builder.HasOne(ps => ps.Page)
            .WithMany(p => p.Sections)
            .HasForeignKey(ps => ps.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ps => new { ps.PageId, ps.Orden });
    }
}

