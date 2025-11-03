using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minimarket.Infrastructure.Data;
using Minimarket.Infrastructure.Data.Seeders;

namespace Minimarket.API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<MinimarketDbContext>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser<Guid>>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            // Aplicar migraciones pendientes
            await context.Database.MigrateAsync();

            // Seed datos iniciales
            await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error al inicializar la base de datos");
        }
    }
}

