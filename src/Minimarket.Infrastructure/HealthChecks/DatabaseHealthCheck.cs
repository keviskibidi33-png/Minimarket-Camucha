using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Minimarket.Infrastructure.Data;

namespace Minimarket.Infrastructure.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly MinimarketDbContext _context;

    public DatabaseHealthCheck(MinimarketDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Intentar conectar y ejecutar una query simple
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy(
                    "Database is not accessible",
                    data: new Dictionary<string, object>
                    {
                        { "database", "SQL Server" },
                        { "status", "unhealthy" }
                    });
            }

            var productsCount = await _context.Products.CountAsync(cancellationToken);
            var categoriesCount = await _context.Categories.CountAsync(cancellationToken);
            var salesCount = await _context.Sales.CountAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "database", "SQL Server" },
                { "status", "healthy" },
                { "products_count", productsCount },
                { "categories_count", categoriesCount },
                { "sales_count", salesCount }
            };

            return HealthCheckResult.Healthy("Database is healthy", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database is unhealthy",
                ex,
                new Dictionary<string, object>
                {
                    { "database", "SQL Server" },
                    { "error", ex.Message },
                    { "status", "unhealthy" }
                });
        }
    }
}

