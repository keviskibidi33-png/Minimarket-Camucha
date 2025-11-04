using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Modules");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Descripcion)
            .HasMaxLength(500);

        builder.Property(m => m.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(m => m.Slug)
            .IsUnique();
    }
}

