using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minimarket.Domain.Interfaces;
using Minimarket.Infrastructure.Data;
using Minimarket.Infrastructure.Data.Repositories;
using Minimarket.Infrastructure.Services;

namespace Minimarket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<MinimarketDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(MinimarketDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: configuration.GetValue<int>("DatabaseSettings:MaxRetryCount", 3),
                    maxRetryDelay: TimeSpan.FromSeconds(
                        configuration.GetValue<int>("DatabaseSettings:MaxRetryDelay", 10)),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(configuration.GetValue<int>("DatabaseSettings:CommandTimeout", 30));
            });

            // Solo en desarrollo
            if (configuration.GetValue<bool>("DatabaseSettings:EnableSensitiveDataLogging", false))
            {
                options.EnableSensitiveDataLogging();
            }

            if (configuration.GetValue<bool>("DatabaseSettings:EnableDetailedErrors", false))
            {
                options.EnableDetailedErrors();
            }
        });

        // Repositories - UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories específicos (también disponibles individualmente)
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();

        // Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}

