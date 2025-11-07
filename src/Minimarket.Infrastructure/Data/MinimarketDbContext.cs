using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Minimarket.Domain.Entities;
using System.Reflection;

namespace Minimarket.Infrastructure.Data;

public class MinimarketDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    public MinimarketDbContext(DbContextOptions<MinimarketDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleDetail> SaleDetails { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }
    public DbSet<SystemSettings> SystemSettings { get; set; }
    public DbSet<ShippingRate> ShippingRates { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<BrandSettings> BrandSettings { get; set; }
    public DbSet<Domain.Entities.Module> Modules { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Sede> Sedes { get; set; }
    public DbSet<Oferta> Ofertas { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<PageSection> PageSections { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<PageView> PageViews { get; set; }
    public DbSet<ProductView> ProductViews { get; set; }
    public DbSet<WebOrder> WebOrders { get; set; }
    public DbSet<WebOrderItem> WebOrderItems { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserPaymentMethod> UserPaymentMethods { get; set; }
    public DbSet<PaymentMethodSettings> PaymentMethodSettings { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure Identity table names
        modelBuilder.Entity<IdentityUser<Guid>>().ToTable("Users");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // Configure global settings
        ConfigureGlobalSettings(modelBuilder);
    }

    private void ConfigureGlobalSettings(ModelBuilder modelBuilder)
    {
        // Configurar eliminación en cascada como restrict por defecto para entidades de dominio
        var domainEntities = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in domainEntities)
        {
            var foreignKeys = entityType.GetForeignKeys();
            foreach (var foreignKey in foreignKeys)
            {
                // Solo aplicar Restrict a relaciones entre entidades de dominio
                // Las relaciones de Identity se mantienen con su comportamiento por defecto
                if (typeof(BaseEntity).IsAssignableFrom(foreignKey.PrincipalEntityType.ClrType))
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }
        }

        // Configurar precisión decimal para propiedades de tipo decimal
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var decimalProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

            foreach (var property in decimalProperties)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasPrecision(18, 2);
            }
        }

        // Configurar índices para búsquedas comunes
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => new { c.DocumentType, c.DocumentNumber })
            .IsUnique();

        modelBuilder.Entity<Sale>()
            .HasIndex(s => s.DocumentNumber)
            .IsUnique();

        modelBuilder.Entity<Sale>()
            .HasIndex(s => s.SaleDate);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Setear timestamps automáticamente para entidades BaseEntity
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }

            entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

