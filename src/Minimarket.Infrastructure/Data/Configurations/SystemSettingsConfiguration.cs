using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class SystemSettingsConfiguration : IEntityTypeConfiguration<SystemSettings>
{
    public void Configure(EntityTypeBuilder<SystemSettings> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.HasIndex(s => s.Category);
    }
}

