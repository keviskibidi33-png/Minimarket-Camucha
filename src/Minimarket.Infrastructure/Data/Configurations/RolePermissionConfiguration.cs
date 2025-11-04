using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.RoleId)
            .IsRequired();

        builder.Property(rp => rp.ModuleId)
            .IsRequired();

        // Relación con Module
        builder.HasOne(rp => rp.Module)
            .WithMany(m => m.RolePermissions)
            .HasForeignKey(rp => rp.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice único para evitar duplicados
        builder.HasIndex(rp => new { rp.RoleId, rp.ModuleId })
            .IsUnique();
    }
}

