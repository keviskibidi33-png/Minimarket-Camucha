using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class SedeConfiguration : IEntityTypeConfiguration<Sede>
{
    public void Configure(EntityTypeBuilder<Sede> builder)
    {
        builder.ToTable("Sedes");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Direccion)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.Ciudad)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Pais)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("Perú");

        builder.Property(s => s.Latitud)
            .HasColumnType("decimal(18,6)");

        builder.Property(s => s.Longitud)
            .HasColumnType("decimal(18,6)");

        builder.Property(s => s.Telefono)
            .HasMaxLength(20);

        builder.Property(s => s.HorariosJson)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.LogoUrl)
            .HasMaxLength(500);

        builder.Property(s => s.Estado)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.GoogleMapsUrl)
            .HasMaxLength(1000);
    }
}

