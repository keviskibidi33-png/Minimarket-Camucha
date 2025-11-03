using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;

namespace Minimarket.Infrastructure.Data.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(MinimarketDbContext context, UserManager<IdentityUser<Guid>> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedCategoriesAsync(context);
        await SeedCustomersAsync(context);
        await SeedProductsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[] { "Administrador", "Cajero", "Almacenero" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<IdentityUser<Guid>> userManager)
    {
        // Admin User
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var admin = new IdentityUser<Guid>
            {
                UserName = "admin",
                Email = "admin@minimarketcamucha.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Administrador");
        }

        // Cajero User
        if (await userManager.FindByNameAsync("cajero") == null)
        {
            var cajero = new IdentityUser<Guid>
            {
                UserName = "cajero",
                Email = "cajero@minimarketcamucha.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(cajero, "Cajero123!");
            await userManager.AddToRoleAsync(cajero, "Cajero");
        }

        // Almacenero User
        if (await userManager.FindByNameAsync("almacenero") == null)
        {
            var almacenero = new IdentityUser<Guid>
            {
                UserName = "almacenero",
                Email = "almacenero@minimarketcamucha.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(almacenero, "Almacenero123!");
            await userManager.AddToRoleAsync(almacenero, "Almacenero");
        }
    }

    private static async Task SeedCategoriesAsync(MinimarketDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new[]
        {
            new Category { Name = "Lácteos", Description = "Leche, queso, mantequilla y derivados" },
            new Category { Name = "Abarrotes", Description = "Arroz, azúcar, aceite, fideos" },
            new Category { Name = "Bebidas", Description = "Gaseosas, jugos, agua, cervezas" },
            new Category { Name = "Golosinas", Description = "Snacks, galletas, chocolates" },
            new Category { Name = "Conservas", Description = "Atún, sardinas, enlatados" },
            new Category { Name = "Limpieza", Description = "Detergentes, jabones, productos de limpieza" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCustomersAsync(MinimarketDbContext context)
    {
        if (await context.Customers.AnyAsync())
            return;

        var customers = new[]
        {
            new Customer { DocumentType = "DNI", DocumentNumber = "12345678", Name = "Juan Pérez", Phone = "999999999", Email = "juan@example.com" },
            new Customer { DocumentType = "DNI", DocumentNumber = "87654321", Name = "María García", Phone = "988888888", Email = "maria@example.com" },
            new Customer { DocumentType = "RUC", DocumentNumber = "20123456789", Name = "Empresa ABC S.A.C.", Phone = "977777777", Email = "contacto@empresa.com" },
            new Customer { DocumentType = "DNI", DocumentNumber = "11223344", Name = "Carlos López", Phone = "966666666" },
            new Customer { DocumentType = "DNI", DocumentNumber = "44332211", Name = "Ana Martínez", Phone = "955555555", Email = "ana@example.com" },
            new Customer { DocumentType = "DNI", DocumentNumber = "55667788", Name = "Luis Rodríguez", Phone = "944444444" },
            new Customer { DocumentType = "DNI", DocumentNumber = "88776655", Name = "Sofía Hernández", Phone = "933333333", Email = "sofia@example.com" },
            new Customer { DocumentType = "DNI", DocumentNumber = "99887766", Name = "Pedro Sánchez", Phone = "922222222" },
            new Customer { DocumentType = "RUC", DocumentNumber = "20234567890", Name = "Tienda XYZ E.I.R.L.", Phone = "911111111", Email = "ventas@tienda.com" },
            new Customer { DocumentType = "DNI", DocumentNumber = "66554433", Name = "Laura Torres", Phone = "900000000", Email = "laura@example.com" }
        };

        await context.Customers.AddRangeAsync(customers);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(MinimarketDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var categories = await context.Categories.ToListAsync();
        var lacteos = categories.First(c => c.Name == "Lácteos");
        var abarrotes = categories.First(c => c.Name == "Abarrotes");
        var bebidas = categories.First(c => c.Name == "Bebidas");
        var golosinas = categories.First(c => c.Name == "Golosinas");
        var conservas = categories.First(c => c.Name == "Conservas");
        var limpieza = categories.First(c => c.Name == "Limpieza");

        var products = new List<Product>
        {
            // Lácteos
            new Product { Code = "LG-EVAP-400", Name = "Leche Gloria Evaporada", Description = "Leche evaporada 400ml", PurchasePrice = 2.50m, SalePrice = 3.50m, Stock = 150, MinimumStock = 20, CategoryId = lacteos.Id },
            new Product { Code = "LV-FRESCA-1L", Name = "Leche Laive Fresca", Description = "Leche fresca 1 litro", PurchasePrice = 2.00m, SalePrice = 2.50m, Stock = 200, MinimumStock = 30, CategoryId = lacteos.Id },
            new Product { Code = "QUESO-ANDINO-250", Name = "Queso Andino", Description = "Queso fresco 250g", PurchasePrice = 4.00m, SalePrice = 5.50m, Stock = 80, MinimumStock = 15, CategoryId = lacteos.Id },
            new Product { Code = "MANTEQUILLA-500", Name = "Mantequilla Laive", Description = "Mantequilla 500g", PurchasePrice = 6.00m, SalePrice = 7.50m, Stock = 60, MinimumStock = 10, CategoryId = lacteos.Id },
            new Product { Code = "YOGURT-GLORIA-1L", Name = "Yogurt Gloria Natural", Description = "Yogurt natural 1 litro", PurchasePrice = 3.50m, SalePrice = 4.50m, Stock = 100, MinimumStock = 20, CategoryId = lacteos.Id },

            // Abarrotes
            new Product { Code = "AR-COS-750", Name = "Arroz Costeño", Description = "Arroz extra 750g", PurchasePrice = 3.00m, SalePrice = 4.20m, Stock = 200, MinimumStock = 50, CategoryId = abarrotes.Id },
            new Product { Code = "ACEITE-PRIMOR-1L", Name = "Aceite Primor 1L", Description = "Aceite vegetal 1 litro", PurchasePrice = 7.00m, SalePrice = 9.80m, Stock = 120, MinimumStock = 25, CategoryId = abarrotes.Id },
            new Product { Code = "AZUCAR-RUBIA-1KG", Name = "Azúcar Rubia", Description = "Azúcar rubia 1kg", PurchasePrice = 2.50m, SalePrice = 3.50m, Stock = 150, MinimumStock = 30, CategoryId = abarrotes.Id },
            new Product { Code = "FIDEOS-DON-VITTORIO-400", Name = "Fideos Don Vittorio", Description = "Fideos tallarín 400g", PurchasePrice = 1.80m, SalePrice = 2.50m, Stock = 180, MinimumStock = 40, CategoryId = abarrotes.Id },
            new Product { Code = "SAL-LOS-ANDES-1KG", Name = "Sal Los Andes", Description = "Sal refinada 1kg", PurchasePrice = 1.20m, SalePrice = 1.80m, Stock = 100, MinimumStock = 20, CategoryId = abarrotes.Id },

            // Bebidas
            new Product { Code = "COCA-COLA-500", Name = "Gaseosa Coca-Cola", Description = "Coca-Cola personal 500ml", PurchasePrice = 1.80m, SalePrice = 2.50m, Stock = 300, MinimumStock = 50, CategoryId = bebidas.Id },
            new Product { Code = "INCA-KOLA-500", Name = "Gaseosa Inca Kola", Description = "Inca Kola personal 500ml", PurchasePrice = 1.80m, SalePrice = 2.50m, Stock = 280, MinimumStock = 50, CategoryId = bebidas.Id },
            new Product { Code = "AGUA-SAN-LUIS-625", Name = "Agua Mineral Sin Gas", Description = "Agua San Luis 625ml", PurchasePrice = 1.00m, SalePrice = 1.50m, Stock = 250, MinimumStock = 40, CategoryId = bebidas.Id },
            new Product { Code = "CERVEZA-PILSEN-620", Name = "Cerveza Pilsen", Description = "Cerveza Pilsen botella 620ml", PurchasePrice = 5.00m, SalePrice = 7.00m, Stock = 200, MinimumStock = 30, CategoryId = bebidas.Id },
            new Product { Code = "CERVEZA-CUSQUEÑA-473", Name = "Cerveza Cusqueña", Description = "Cerveza Cusqueña lata 473ml", PurchasePrice = 4.00m, SalePrice = 5.50m, Stock = 180, MinimumStock = 30, CategoryId = bebidas.Id },

            // Golosinas
            new Product { Code = "GALLETAS-CASINO-6P", Name = "Galletas Casino", Description = "Galletas Casino paquete 6 unidades", PurchasePrice = 0.90m, SalePrice = 1.20m, Stock = 150, MinimumStock = 30, CategoryId = golosinas.Id },
            new Product { Code = "CHIPS-AHoy-160", Name = "Chips Ahoy", Description = "Galletas Chips Ahoy 160g", PurchasePrice = 2.50m, SalePrice = 3.50m, Stock = 100, MinimumStock = 20, CategoryId = golosinas.Id },
            new Product { Code = "CHOCOLATE-COSTA-100", Name = "Chocolate Costa", Description = "Chocolate con leche 100g", PurchasePrice = 1.50m, SalePrice = 2.00m, Stock = 120, MinimumStock = 25, CategoryId = golosinas.Id },
            new Product { Code = "DORITOS-150", Name = "Doritos", Description = "Doritos nachos 150g", PurchasePrice = 2.00m, SalePrice = 2.80m, Stock = 90, MinimumStock = 20, CategoryId = golosinas.Id },
            new Product { Code = "SNICKERS-52", Name = "Snickers", Description = "Barra Snickers 52g", PurchasePrice = 1.80m, SalePrice = 2.50m, Stock = 200, MinimumStock = 40, CategoryId = golosinas.Id },

            // Conservas
            new Product { Code = "ATUN-FLORIDA-170", Name = "Atún Florida 170g", Description = "Atún en aceite vegetal 170g", PurchasePrice = 4.00m, SalePrice = 5.50m, Stock = 150, MinimumStock = 30, CategoryId = conservas.Id },
            new Product { Code = "SARDINAS-REAL-170", Name = "Sardinas Real", Description = "Sardinas en aceite 170g", PurchasePrice = 2.50m, SalePrice = 3.50m, Stock = 120, MinimumStock = 25, CategoryId = conservas.Id },
            new Product { Code = "ATUN-PRIMOR-170", Name = "Atún Primor", Description = "Atún en agua 170g", PurchasePrice = 3.80m, SalePrice = 5.00m, Stock = 100, MinimumStock = 20, CategoryId = conservas.Id },

            // Limpieza
            new Product { Code = "DETERGENTE-ACE-750", Name = "Detergente Ace", Description = "Detergente líquido Ace 750ml", PurchasePrice = 4.50m, SalePrice = 6.00m, Stock = 80, MinimumStock = 15, CategoryId = limpieza.Id },
            new Product { Code = "JABON-LAVA-200", Name = "Jabón Lava", Description = "Jabón en barra Lava 200g", PurchasePrice = 1.50m, SalePrice = 2.20m, Stock = 100, MinimumStock = 20, CategoryId = limpieza.Id },
            new Product { Code = "PAPEL-HIGIENICO-SUPER-12", Name = "Papel Higiénico Super", Description = "Papel higiénico 12 rollos", PurchasePrice = 8.00m, SalePrice = 11.00m, Stock = 60, MinimumStock = 12, CategoryId = limpieza.Id }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}

