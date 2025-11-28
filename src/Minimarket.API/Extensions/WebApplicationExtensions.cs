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
            var logger = services.GetRequiredService<ILogger<Program>>();

            // Aplicar migraciones pendientes
            logger.LogInformation("Aplicando migraciones pendientes...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas exitosamente");

            // Seed datos iniciales
            await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error al inicializar la base de datos: {Message}", ex.Message);
            logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            // No lanzar la excepción para que la aplicación pueda iniciar
            // pero registrar el error para debugging
        }
    }
}

