using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class OfertaConfiguration : IEntityTypeConfiguration<Oferta>
{
    public void Configure(EntityTypeBuilder<Oferta> builder)
    {
        builder.ToTable("Ofertas");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Descripcion)
            .HasMaxLength(1000);

        builder.Property(o => o.DescuentoTipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.DescuentoValor)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.CategoriasIdsJson)
            .IsRequired()
            .HasMaxLength(2000)
            .HasDefaultValue("[]");

        builder.Property(o => o.ProductosIdsJson)
            .IsRequired()
            .HasMaxLength(2000)
            .HasDefaultValue("[]");

        builder.Property(o => o.FechaInicio)
            .IsRequired();

        builder.Property(o => o.FechaFin)
            .IsRequired();

        builder.Property(o => o.Activa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(o => o.Orden)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(o => o.Orden);
        builder.HasIndex(o => new { o.FechaInicio, o.FechaFin });
    }
}

