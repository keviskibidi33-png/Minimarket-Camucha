using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minimarket.Domain.Entities;

namespace Minimarket.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.DocumentType)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.DocumentNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .HasMaxLength(100);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.HasIndex(c => new { c.DocumentType, c.DocumentNumber })
            .IsUnique();
    }
}

