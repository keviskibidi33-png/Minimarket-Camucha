using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Minimarket.API;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Infrastructure.Data;
using System.Text.Json;

namespace Minimarket.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove real database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MinimarketDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database for testing
            services.AddDbContext<MinimarketDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Create database and seed test data
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MinimarketDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            db.Database.EnsureCreated();
            SeedTestData(db, userManager, roleManager).Wait();
        });
    }

    private async Task SeedTestData(MinimarketDbContext context, UserManager<IdentityUser<Guid>> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        // Seed roles
        if (!await roleManager.RoleExistsAsync("Administrador"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Administrador" });
        }
        if (!await roleManager.RoleExistsAsync("Cajero"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Cajero" });
        }
        if (!await roleManager.RoleExistsAsync("Almacenero"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Almacenero" });
        }

        // Seed test user
        var testUser = await userManager.FindByNameAsync("testuser@minimarket.com");
        if (testUser == null)
        {
            testUser = new IdentityUser<Guid>
            {
                Id = Guid.NewGuid(),
                UserName = "testuser@minimarket.com",
                Email = "testuser@minimarket.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(testUser, "Test@1234");
            await userManager.AddToRoleAsync(testUser, "Administrador");
        }

        // Seed categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Id = Guid.NewGuid(), Name = "Bebidas", Description = "Bebidas", IsActive = true },
                new() { Id = Guid.NewGuid(), Name = "Snacks", Description = "Snacks", IsActive = true },
                new() { Id = Guid.NewGuid(), Name = "Lacteos", Description = "Lacteos", IsActive = true }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            // Seed products
            var products = new List<Product>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "7750001001001",
                    Name = "Coca Cola 500ml",
                    PurchasePrice = 2.00m,
                    SalePrice = 3.50m,
                    Stock = 100,
                    MinimumStock = 10,
                    CategoryId = categories[0].Id,
                    IsActive = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "7750001001002",
                    Name = "Papas Lays 150g",
                    PurchasePrice = 3.00m,
                    SalePrice = 5.00m,
                    Stock = 50,
                    MinimumStock = 5,
                    CategoryId = categories[1].Id,
                    IsActive = true
                }
            };
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        // Seed customers
        if (!await context.Customers.AnyAsync())
        {
            var customers = new List<Customer>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    DocumentType = "DNI",
                    DocumentNumber = "12345678",
                    Name = "Juan Pérez",
                    Email = "juan@example.com",
                    Phone = "987654321",
                    IsActive = true
                }
            };
            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();
        }
    }

    public async Task InitializeAsync()
    {
        // Setup for testing if needed
        await Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}

// Collection definition for xUnit
[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}

