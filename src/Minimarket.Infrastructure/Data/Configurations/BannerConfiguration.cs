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

        builder.Property(b => b.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Descripcion)
            .HasMaxLength(1000);

        builder.Property(b => b.ImagenUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.UrlDestino)
            .HasMaxLength(500);

        builder.Property(b => b.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.Posicion)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.Orden)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.ClasesCss)
            .HasMaxLength(500);

        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.DeletedAt)
            .IsRequired(false);

        builder.HasIndex(b => b.Tipo);
        builder.HasIndex(b => b.Posicion);
        builder.HasIndex(b => b.Orden);
        builder.HasIndex(b => new { b.Activo, b.FechaInicio, b.FechaFin });
    }
}
