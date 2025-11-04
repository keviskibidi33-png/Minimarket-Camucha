using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> builder)
    {
        builder.ToTable("Translations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("general");

        // Índice único para Key + LanguageCode + Category
        builder.HasIndex(t => new { t.Key, t.LanguageCode, t.Category })
            .IsUnique();
    }
}

