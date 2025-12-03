using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using System.Collections.Generic;

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
        await SeedSystemSettingsAsync(context);
        await SeedShippingRatesAsync(context);
        await SeedModulesAsync(context);
        await SeedBrandSettingsAsync(context);
        await SeedPaymentMethodSettingsAsync(context);
        await SeedPagesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[] { "Administrador", "Cajero", "Almacenero", "Cliente" };

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
        var admin = await userManager.FindByNameAsync("admin") ?? await userManager.FindByEmailAsync("admin@minimarketcamucha.com");
        if (admin == null)
        {
            admin = new IdentityUser<Guid>
            {
                UserName = "admin",
                Email = "admin@minimarketcamucha.com",
                EmailConfirmed = true,
                LockoutEnabled = false // Deshabilitar bloqueo para admin inicial en producción
            };
            var createResult = await userManager.CreateAsync(admin, "Admin123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Administrador");
            }
        }
        else
        {
            // Asegurar que el admin existente tenga EmailConfirmed y no esté bloqueado
            if (!admin.EmailConfirmed)
            {
                admin.EmailConfirmed = true;
                await userManager.UpdateAsync(admin);
            }
            if (admin.LockoutEnabled && admin.LockoutEnd.HasValue && admin.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                // Desbloquear admin si está bloqueado
                admin.LockoutEnd = null;
                await userManager.UpdateAsync(admin);
            }
            // Asegurar que tenga el rol Administrador
            var roles = await userManager.GetRolesAsync(admin);
            if (!roles.Contains("Administrador"))
            {
                await userManager.AddToRoleAsync(admin, "Administrador");
            }
        }

        // Cajero User
        var cajero = await userManager.FindByNameAsync("cajero") ?? await userManager.FindByEmailAsync("cajero@minimarketcamucha.com");
        if (cajero == null)
        {
            cajero = new IdentityUser<Guid>
            {
                UserName = "cajero",
                Email = "cajero@minimarketcamucha.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(cajero, "Cajero123!");
            await userManager.AddToRoleAsync(cajero, "Cajero");
        }

        // Almacenero User
        var almacenero = await userManager.FindByNameAsync("almacenero") ?? await userManager.FindByEmailAsync("almacenero@minimarketcamucha.com");
        if (almacenero == null)
        {
            almacenero = new IdentityUser<Guid>
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

    private static async Task SeedSystemSettingsAsync(MinimarketDbContext context)
    {
        if (await context.SystemSettings.AnyAsync())
            return;

        var settings = new[]
        {
            new SystemSettings
            {
                Key = "apply_igv_to_cart",
                Value = "false",
                Description = "Aplicar IGV (18%) al carrito de compras. Si está activado, el IGV se mostrará y aplicará en el carrito.",
                Category = "cart",
                IsActive = true
            },
            new SystemSettings
            {
                Key = "enable_news_in_navbar",
                Value = "true",
                Description = "Activar o desactivar la funcionalidad de mostrar noticias en el navbar. Si está desactivado, ninguna noticia aparecerá en el navbar.",
                Category = "navbar",
                IsActive = true
            }
        };

        await context.SystemSettings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
    }

    private static async Task SeedShippingRatesAsync(MinimarketDbContext context)
    {
        if (await context.ShippingRates.AnyAsync())
            return;

        // Tarifas de envío basadas en datos reales de Lima, Perú
        // Considerando: combustible, distancia, peso, y costos operativos
        var shippingRates = new[]
        {
            // Lima Centro - Zona más cercana, tarifas más económicas
            new ShippingRate
            {
                ZoneName = "Lima Centro",
                BasePrice = 3.50m,        // Costo base de envío
                PricePerKm = 0.50m,       // Precio adicional por kilómetro
                PricePerKg = 1.00m,       // Precio adicional por kilogramo
                MinDistance = 0m,
                MaxDistance = 10m,        // Hasta 10 km
                MinWeight = 0m,
                MaxWeight = 0m,          // Sin límite de peso
                FreeShippingThreshold = 100.00m, // Envío gratis si compra >= S/ 100
                IsActive = true
            },
            // Lima Norte - Distancias mayores, tarifa media
            new ShippingRate
            {
                ZoneName = "Lima Norte",
                BasePrice = 4.00m,
                PricePerKm = 0.60m,
                PricePerKg = 1.20m,
                MinDistance = 0m,
                MaxDistance = 20m,        // Hasta 20 km
                MinWeight = 0m,
                MaxWeight = 0m,
                FreeShippingThreshold = 100.00m,
                IsActive = true
            },
            // Lima Sur - Similar a Lima Norte
            new ShippingRate
            {
                ZoneName = "Lima Sur",
                BasePrice = 4.00m,
                PricePerKm = 0.60m,
                PricePerKg = 1.20m,
                MinDistance = 0m,
                MaxDistance = 20m,
                MinWeight = 0m,
                MaxWeight = 0m,
                FreeShippingThreshold = 100.00m,
                IsActive = true
            },
            // Callao - Zona cercana pero con peaje adicional
            new ShippingRate
            {
                ZoneName = "Callao",
                BasePrice = 4.50m,
                PricePerKm = 0.70m,
                PricePerKg = 1.30m,
                MinDistance = 0m,
                MaxDistance = 15m,
                MinWeight = 0m,
                MaxWeight = 0m,
                FreeShippingThreshold = 100.00m,
                IsActive = true
            },
            // Lima Este - Zona más alejada, tarifa más alta
            new ShippingRate
            {
                ZoneName = "Lima Este",
                BasePrice = 5.00m,
                PricePerKm = 0.80m,
                PricePerKg = 1.50m,
                MinDistance = 0m,
                MaxDistance = 25m,
                MinWeight = 0m,
                MaxWeight = 0m,
                FreeShippingThreshold = 100.00m,
                IsActive = true
            },
            // Lima Oeste - Zona costera, tarifa media-alta
            new ShippingRate
            {
                ZoneName = "Lima Oeste",
                BasePrice = 4.50m,
                PricePerKm = 0.65m,
                PricePerKg = 1.25m,
                MinDistance = 0m,
                MaxDistance = 18m,
                MinWeight = 0m,
                MaxWeight = 0m,
                FreeShippingThreshold = 100.00m,
                IsActive = true
            },
            // Zona General - Para distancias mayores o zonas no especificadas
            new ShippingRate
            {
                ZoneName = "Zona General",
                BasePrice = 5.50m,
                PricePerKm = 1.00m,
                PricePerKg = 2.00m,
                MinDistance = 0m,
                MaxDistance = 0m,        // Sin límite de distancia
                MinWeight = 0m,
                MaxWeight = 0m,
                FreeShippingThreshold = 150.00m, // Envío gratis si compra >= S/ 150
                IsActive = true
            }
        };

        await context.ShippingRates.AddRangeAsync(shippingRates);
        await context.SaveChangesAsync();
    }

    private static async Task SeedModulesAsync(MinimarketDbContext context)
    {
        if (await context.Modules.AnyAsync())
            return;

        var modules = new[]
        {
            new Domain.Entities.Module
            {
                Nombre = "Configuración - Edición",
                Descripcion = "Gestionar configuración de marca (logo, colores, datos de contacto)",
                Slug = "configuracion_edicion",
                IsActive = true
            },
            new Domain.Entities.Module
            {
                Nombre = "Configuración - Permisos",
                Descripcion = "Gestionar roles y permisos granulares",
                Slug = "configuracion_permisos",
                IsActive = true
            },
            new Domain.Entities.Module
            {
                Nombre = "Sedes",
                Descripcion = "Gestionar sedes y ubicaciones",
                Slug = "sedes",
                IsActive = true
            },
            new Domain.Entities.Module
            {
                Nombre = "Productos",
                Descripcion = "Gestionar productos y catálogo",
                Slug = "productos",
                IsActive = true
            },
            new Domain.Entities.Module
            {
                Nombre = "Categorías",
                Descripcion = "Gestionar categorías de productos",
                Slug = "categorias",
                IsActive = true
            },
            new Domain.Entities.Module
            {
                Nombre = "Ofertas",
                Descripcion = "Gestionar ofertas y descuentos",
                Slug = "ofertas",
                IsActive = true
            },
            new Domain.Entities.Module
            {
                Nombre = "Page Builder",
                Descripcion = "Constructor de páginas modular",
                Slug = "page_builder",
                IsActive = true
            },
            new Domain.Entities.Module
            {
                Nombre = "Analytics",
                Descripcion = "Ver estadísticas y reportes",
                Slug = "analytics",
                IsActive = true
            },
            new Domain.Entities.Module
            {
                Nombre = "Ventas",
                Descripcion = "Gestionar ventas y transacciones",
                Slug = "ventas",
                IsActive = true
            },
            new Domain.Entities.Module
            {
                Nombre = "Clientes",
                Descripcion = "Gestionar clientes",
                Slug = "clientes",
                IsActive = true
            },
            new Domain.Entities.Module
            {
                Nombre = "Usuarios",
                Descripcion = "Gestionar usuarios del sistema",
                Slug = "usuarios",
                IsActive = true
            }
        };

        await context.Modules.AddRangeAsync(modules);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBrandSettingsAsync(MinimarketDbContext context)
    {
        if (await context.BrandSettings.AnyAsync())
            return;

        var brandSettings = new Domain.Entities.BrandSettings
        {
            LogoUrl = string.Empty,
            StoreName = "Minimarket Camucha",
            PrimaryColor = "#4CAF50",
            SecondaryColor = "#0d7ff2",
            ButtonColor = "#4CAF50",
            TextColor = "#333333",
            HoverColor = "#45a049",
            Description = "Tu minimarket de confianza",
            Slogan = "Calidad y servicio siempre",
            Phone = "+51 999 999 999",
            Email = "contacto@minimarketcamucha.com",
            Address = "Av. Principal 123, Lima, Perú",
            UpdatedBy = Guid.Empty // Se actualizará cuando un usuario real lo modifique
        };

        await context.BrandSettings.AddAsync(brandSettings);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPaymentMethodSettingsAsync(MinimarketDbContext context)
    {
        // Solo crear si no existen registros
        if (await context.PaymentMethodSettings.AnyAsync())
            return;

        var paymentMethods = new[]
        {
            new PaymentMethodSettings
            {
                PaymentMethodId = (int)PaymentMethod.Efectivo,
                Name = "Efectivo",
                IsEnabled = true,
                RequiresCardDetails = false,
                Description = "Pago en efectivo al momento de la entrega",
                DisplayOrder = 1
            },
            new PaymentMethodSettings
            {
                PaymentMethodId = (int)PaymentMethod.YapePlin,
                Name = "Yape/Plin",
                IsEnabled = true,
                RequiresCardDetails = false,
                Description = "Pago mediante Yape o Plin",
                DisplayOrder = 2
            },
            new PaymentMethodSettings
            {
                PaymentMethodId = (int)PaymentMethod.Transferencia,
                Name = "Transferencia",
                IsEnabled = true,
                RequiresCardDetails = false,
                Description = "Transferencia bancaria",
                DisplayOrder = 3
            }
        };

        await context.PaymentMethodSettings.AddRangeAsync(paymentMethods);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPagesAsync(MinimarketDbContext context)
    {
        // Verificar si las páginas ya existen
        var existingPages = await context.Pages.Where(p => p.Slug == "acerca-de-nosotros" || p.Slug == "informacion-de-envio").ToListAsync();
        if (existingPages.Any())
            return;

        // Obtener BrandSettings para usar datos de contacto
        var brandSettings = await context.BrandSettings.FirstOrDefaultAsync();
        var companyPhone = brandSettings?.Phone ?? "+51 999 999 999";
        var companyEmail = brandSettings?.Email ?? "minimarket.camucha@gmail.com";
        var companyAddress = brandSettings?.Address ?? "Jr. Pedro Labarthe 449 – Ingeniería, San Martín de Porres, Lima, Lima, Perú";

        // Página: Acerca de Nosotros
        var acercaPage = new Page
        {
            Titulo = "Acerca de Nosotros",
            Slug = "acerca-de-nosotros",
            TipoPlantilla = TipoPlantilla.Generica,
            MetaDescription = "Conoce más sobre Minimarket Camucha, tu tienda de confianza en San Martín de Porres, Lima.",
            Keywords = "minimarket, camucha, tienda, abarrotes, san martín de porres, lima",
            Orden = 1,
            Activa = true,
            MostrarEnNavbar = false
        };

        await context.Pages.AddAsync(acercaPage);
        await context.SaveChangesAsync();

        // Sección 1: Banner
        var acercaBanner = new PageSection
        {
            PageId = acercaPage.Id,
            SeccionTipo = SeccionTipo.Banner,
            Orden = 0
        };
        acercaBanner.SetDatos(new Dictionary<string, object>
        {
            { "titulo", "Acerca de Nosotros" },
            { "contenido", "<p>Tu minimarket de confianza en San Martín de Porres</p>" },
            { "imagenUrl", "" }
        });

        // Sección 2: Texto e Imagen
        var acercaTexto = new PageSection
        {
            PageId = acercaPage.Id,
            SeccionTipo = SeccionTipo.TextoImagen,
            Orden = 1
        };
        acercaTexto.SetDatos(new Dictionary<string, object>
        {
            { "titulo", "Nuestra Historia" },
            { "contenido", @"<p>Minimarket Camucha es una tienda familiar que nació con el propósito de brindar productos de calidad y un servicio excepcional a nuestros clientes en San Martín de Porres, Lima.</p>
<p>Desde nuestros inicios, nos hemos comprometido a ofrecer una amplia variedad de productos de primera necesidad, desde abarrotes y bebidas hasta productos de limpieza y cuidado personal.</p>
<p>Nuestro compromiso es mantener siempre los mejores precios, productos frescos y un trato amable y profesional con cada uno de nuestros clientes.</p>
<h3>Nuestra Misión</h3>
<p>Proporcionar productos de calidad a precios justos, con un servicio al cliente excepcional que satisfaga las necesidades de nuestras familias y comunidad.</p>
<h3>Nuestros Valores</h3>
<ul>
<li><strong>Calidad:</strong> Seleccionamos cuidadosamente cada producto que ofrecemos.</li>
<li><strong>Confianza:</strong> Construimos relaciones duraderas con nuestros clientes.</li>
<li><strong>Servicio:</strong> Nos esforzamos por brindar la mejor experiencia de compra.</li>
<li><strong>Compromiso:</strong> Con nuestra comunidad y el bienestar de nuestros clientes.</li>
</ul>" },
            { "imagenUrl", "" },
            { "posicion", "left" }
        });

        await context.PageSections.AddRangeAsync(acercaBanner, acercaTexto);

        // Página: Información de Envío
        var envioPage = new Page
        {
            Titulo = "Información de Envío",
            Slug = "informacion-de-envio",
            TipoPlantilla = TipoPlantilla.Generica,
            MetaDescription = "Información sobre nuestros servicios de envío y entrega a domicilio en Lima, Perú.",
            Keywords = "envío, entrega, domicilio, delivery, lima, perú, minimarket camucha",
            Orden = 2,
            Activa = true,
            MostrarEnNavbar = false
        };

        await context.Pages.AddAsync(envioPage);
        await context.SaveChangesAsync();

        // Sección 1: Banner
        var envioBanner = new PageSection
        {
            PageId = envioPage.Id,
            SeccionTipo = SeccionTipo.Banner,
            Orden = 0
        };
        envioBanner.SetDatos(new Dictionary<string, object>
        {
            { "titulo", "Información de Envío" },
            { "contenido", "<p>Llevamos tus compras hasta la puerta de tu hogar</p>" },
            { "imagenUrl", "https://images.unsplash.com/photo-1607083206869-4c7672e72a8a?w=1920&q=80" }
        });

        // Sección 2: Texto e Imagen - Información General
        var envioInfo = new PageSection
        {
            PageId = envioPage.Id,
            SeccionTipo = SeccionTipo.TextoImagen,
            Orden = 1
        };
        envioInfo.SetDatos(new Dictionary<string, object>
        {
            { "titulo", "Servicio de Entrega a Domicilio" },
            { "contenido", @"<p>En Minimarket Camucha ofrecemos servicio de entrega a domicilio para tu comodidad. Realizamos entregas en diferentes zonas de Lima, con tarifas accesibles y tiempos de entrega rápidos.</p>
<h3>Zonas de Cobertura</h3>
<ul>
<li><strong>Lima Centro:</strong> Hasta 10 km - Tarifa desde S/ 3.50</li>
<li><strong>Lima Norte:</strong> Hasta 20 km - Tarifa desde S/ 4.00</li>
<li><strong>Lima Sur:</strong> Hasta 20 km - Tarifa desde S/ 4.00</li>
<li><strong>Callao:</strong> Hasta 15 km - Tarifa desde S/ 4.50</li>
<li><strong>Lima Este:</strong> Hasta 25 km - Tarifa desde S/ 5.00</li>
<li><strong>Lima Oeste:</strong> Hasta 18 km - Tarifa desde S/ 4.50</li>
<li><strong>Zona General:</strong> Sin límite de distancia - Tarifa desde S/ 5.50</li>
</ul>
<p>El costo final del envío se calcula según la distancia y el peso de tu pedido. Puedes calcular el costo exacto durante el proceso de checkout.</p>" },
            { "imagenUrl", "" },
            { "posicion", "left" }
        });

        // Sección 3: Texto e Imagen - Envío Gratis
        var envioGratis = new PageSection
        {
            PageId = envioPage.Id,
            SeccionTipo = SeccionTipo.TextoImagen,
            Orden = 2
        };
        envioGratis.SetDatos(new Dictionary<string, object>
        {
            { "titulo", "Envío Gratis" },
            { "contenido", @"<p>¡Aprovecha nuestro servicio de envío gratis!</p>
<ul>
<li><strong>Lima Centro, Norte, Sur, Callao, Este y Oeste:</strong> Envío gratis en compras mayores a S/ 100.00</li>
<li><strong>Zona General:</strong> Envío gratis en compras mayores a S/ 150.00</li>
</ul>
<p>El envío gratis se aplica automáticamente cuando tu pedido alcanza el monto mínimo requerido para tu zona.</p>" },
            { "imagenUrl", "" },
            { "posicion", "right" }
        });

        // Sección 4: Texto e Imagen - Tiempos de Entrega
        var envioTiempos = new PageSection
        {
            PageId = envioPage.Id,
            SeccionTipo = SeccionTipo.TextoImagen,
            Orden = 3
        };
        envioTiempos.SetDatos(new Dictionary<string, object>
        {
            { "titulo", "Tiempos de Entrega" },
            { "contenido", @"<p>Nos esforzamos por entregar tus pedidos lo más rápido posible:</p>
<ul>
<li><strong>Pedidos realizados antes de las 2:00 PM:</strong> Entrega el mismo día (sujeto a disponibilidad)</li>
<li><strong>Pedidos realizados después de las 2:00 PM:</strong> Entrega al día siguiente</li>
<li><strong>Fines de semana:</strong> Entregas disponibles según disponibilidad</li>
</ul>
<p>Los tiempos de entrega pueden variar según la zona y el tráfico. Te notificaremos cuando tu pedido esté en camino.</p>" },
            { "imagenUrl", "" },
            { "posicion", "left" }
        });

        // Sección 5: Texto e Imagen - Información de Contacto
        var envioContacto = new PageSection
        {
            PageId = envioPage.Id,
            SeccionTipo = SeccionTipo.TextoImagen,
            Orden = 4
        };
        envioContacto.SetDatos(new Dictionary<string, object>
        {
            { "titulo", "¿Necesitas más información?" },
            { "contenido", $@"<p>Si tienes alguna pregunta sobre nuestros servicios de envío, no dudes en contactarnos:</p>
<ul>
<li><strong>Dirección:</strong> {companyAddress}</li>
<li><strong>Email:</strong> <a href='mailto:{companyEmail}'>{companyEmail}</a></li>
<li><strong>Teléfono:</strong> {companyPhone}</li>
</ul>
<p>Estamos aquí para ayudarte y resolver todas tus dudas sobre nuestros servicios de entrega.</p>" },
            { "imagenUrl", "" },
            { "posicion", "right" }
        });

        await context.PageSections.AddRangeAsync(envioBanner, envioInfo, envioGratis, envioTiempos, envioContacto);
        await context.SaveChangesAsync();
    }
}

