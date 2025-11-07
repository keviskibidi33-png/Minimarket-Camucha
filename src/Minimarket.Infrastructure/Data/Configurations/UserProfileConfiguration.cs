using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(up => up.Id);

        builder.Property(up => up.UserId)
            .IsRequired();

        builder.Property(up => up.FirstName)
            .HasMaxLength(100);

        builder.Property(up => up.LastName)
            .HasMaxLength(100);

        builder.Property(up => up.Dni)
            .HasMaxLength(8); // DNI peruano tiene 8 dígitos

        builder.Property(up => up.Phone)
            .HasMaxLength(20);

        builder.Property(up => up.ProfileCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Índice único para UserId (un usuario solo puede tener un perfil)
        builder.HasIndex(up => up.UserId)
            .IsUnique();

        // Índice único para DNI (un DNI solo puede estar asociado a un usuario)
        builder.HasIndex(up => up.Dni)
            .IsUnique()
            .HasFilter("[Dni] IS NOT NULL");
    }
}

