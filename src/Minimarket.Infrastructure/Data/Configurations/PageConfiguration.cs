using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("Pages");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.TipoPlantilla)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(500);

        builder.Property(p => p.Keywords)
            .HasMaxLength(500);

        builder.Property(p => p.Orden)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.Activa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.MostrarEnNavbar)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.HasIndex(p => p.Orden);
    }
}

