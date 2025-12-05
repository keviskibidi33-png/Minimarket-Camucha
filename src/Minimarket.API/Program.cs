using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minimarket.API.Extensions;
using Minimarket.API.Middleware;
using Minimarket.Application;
using Minimarket.Infrastructure;
using Minimarket.Infrastructure.Data;
using Serilog;
using System.Diagnostics;
using System.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("TraceId", Activity.Current?.Id ?? "N/A")
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/minimarket-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{TraceId}] {Message:lj}{NewLine}{Exception}");
});

// Configurar límites de tamaño para formularios multipart (subida de archivos)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
    options.ValueLengthLimit = 10 * 1024 * 1024; // 10MB
    options.MultipartHeadersLengthLimit = 1024 * 1024; // 1MB para headers
});

// Add services to the container
builder.Services.AddControllers(options =>
    {
        // Deshabilitar negociación de contenido estricta para evitar errores 406
        options.ReturnHttpNotAcceptable = false;
    })
    .AddJsonOptions(options =>
    {
        // Configurar serialización JSON con buenas prácticas
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        // NO ignorar nulls para asegurar que los datos se serialicen correctamente
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Configuraciones de API behavior (MaxModelBindingCollectionSize fue removido en .NET 9.0)
        // El límite de tamaño de colección se maneja ahora a través de RequestFormLimits
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new
                {
                    Field = x.Key,
                    Message = e.ErrorMessage
                }))
                .ToList();

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
            {
                succeeded = false,
                message = "Error de validación",
                errors = errors
            });
        };
    });

// Static files (para servir imágenes)
builder.Services.AddDirectoryBrowser();

// HttpClient para descargar imágenes
builder.Services.AddHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Minimarket Camucha API",
        Version = "v1",
        Description = "API para sistema de gestión y ventas de minimarket"
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<Minimarket.Infrastructure.HealthChecks.DatabaseHealthCheck>("database_custom", tags: new[] { "ready", "db" });

// Database
builder.Services.AddInfrastructure(builder.Configuration);

// Application Layer
builder.Services.AddApplication();

// Identity
builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Token provider settings - Configurar expiración de tokens a 15 minutos
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
})
.AddEntityFrameworkStores<MinimarketDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<Minimarket.Infrastructure.Identity.SpanishIdentityErrorDescriber>();

// Configurar expiración del token de recuperación de contraseña a 15 minutos
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(15);
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

var googleOAuth = builder.Configuration.GetSection("GoogleOAuth");
var googleClientId = googleOAuth["ClientId"];
var googleClientSecret = googleOAuth["ClientSecret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        // Configurar el mapeo de roles para que ASP.NET Core reconozca los roles del token
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
    };
})
.AddGoogle(options =>
{
    if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/api/auth/google-callback";
    }
});

// CORS - Configuración para producción y desarrollo
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:4200" };

// En producción, usar solo los orígenes configurados en variables de entorno
// En desarrollo, permitir localhost por defecto
var allOrigins = allowedOrigins;

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        // Permitir orígenes específicos configurados
        policy.SetIsOriginAllowed(origin => 
        {
            // Permitir localhost para desarrollo (cualquier puerto)
            if (origin.StartsWith("http://localhost") || 
                origin.StartsWith("https://localhost") ||
                origin.StartsWith("http://127.0.0.1") ||
                origin.StartsWith("https://127.0.0.1"))
                return true;
            
            // Permitir orígenes de producción configurados (comparación case-insensitive)
            if (allOrigins.Any(o => string.Equals(o, origin, StringComparison.OrdinalIgnoreCase)))
                return true;
            
            // Permitir el dominio de producción si está configurado en BaseUrl o FrontendUrl
            var baseUrl = builder.Configuration["BaseUrl"] ?? builder.Configuration["FrontendUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                try
                {
                    var uri = new Uri(baseUrl);
                    var originUri = new Uri(origin);
                    if (uri.Scheme == originUri.Scheme && uri.Host == originUri.Host)
                        return true;
                }
                catch { /* Ignorar errores de parsing */ }
            }
            
            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromHours(24)); // Cache preflight requests
    });
    
    // Política adicional para desarrollo (más permisiva)
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    }
});

// Authorization con permisos granulares
builder.Services.AddAuthorization(options =>
{
    // Las políticas se crearán dinámicamente cuando se use RequirePermissionAttribute
    // El handler PermissionAuthorizationHandler manejará los requirements
});

// Registrar el provider de políticas personalizado para permisos granulares
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, Minimarket.Application.Common.Authorization.PermissionPolicyProvider>();

// Registrar el handler de autorización de permisos
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Minimarket.Application.Common.Authorization.PermissionAuthorizationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// En producción, no exponer Swagger
if (app.Environment.IsProduction())
{
    app.UseHsts();
}

// Security Headers Middleware - debe ir temprano en el pipeline
app.UseMiddleware<SecurityHeadersMiddleware>();

// Correlation ID Middleware - debe ir antes del exception handler
app.UseMiddleware<CorrelationIdMiddleware>();

// Global Exception Handler Middleware - debe ir al inicio del pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Response Logging Middleware - para diagnosticar problemas de respuestas
app.UseMiddleware<ResponseLoggingMiddleware>();

app.UseHttpsRedirection();

// Static files para servir imágenes de templates y uploads
app.UseStaticFiles();

// Configurar archivos estáticos de uploads con ruta específica
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    // Crear subdirectorios comunes
    Directory.CreateDirectory(Path.Combine(uploadsPath, "products"));
    Directory.CreateDirectory(Path.Combine(uploadsPath, "payment-qr"));
    Directory.CreateDirectory(Path.Combine(uploadsPath, "categories"));
    Directory.CreateDirectory(Path.Combine(uploadsPath, "banners"));
    Directory.CreateDirectory(Path.Combine(uploadsPath, "sedes"));
}
// Asegurar que los subdirectorios existan incluso si el directorio principal ya existe
var subdirectories = new[] { "products", "payment-qr", "categories", "banners", "sedes" };
foreach (var subdir in subdirectories)
{
    var subdirPath = Path.Combine(uploadsPath, subdir);
    if (!Directory.Exists(subdirPath))
    {
        Directory.CreateDirectory(subdirPath);
    }
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = "",
    OnPrepareResponse = ctx =>
    {
        // Permitir CORS para archivos estáticos (imágenes)
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        // Cache para archivos estáticos
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
    }
});

// Configurar ruta específica para /uploads/ con mejor manejo de errores
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Permitir CORS para archivos estáticos (imágenes)
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        // Cache para archivos estáticos
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
    },
    ServeUnknownFileTypes = true // Permitir servir cualquier tipo de archivo
});

// CORS - Usar política permisiva en desarrollo, específica en producción
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll"); // Más permisivo para desarrollo
}
else
{
    app.UseCors("FrontendPolicy"); // Política específica para producción
}

app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapControllers();

// Servir frontend Angular compilado (solo si existe)
// Esto permite que backend y frontend funcionen en el mismo puerto
var frontendPath = Path.Combine(builder.Environment.ContentRootPath, "..", "..", "minimarket-web", "dist", "minimarket-web", "browser");
if (Directory.Exists(frontendPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath),
        RequestPath = ""
    });
    
    // Fallback para SPA routing - todas las rutas que no sean API devuelven index.html
    // IMPORTANTE: Esto debe ir DESPUÉS de MapControllers para que las rutas /api/* tengan prioridad
    // También debe ir ANTES de app.Run() pero DESPUÉS de todas las rutas de API
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath)
    });
}

// Seed database on startup
await app.SeedDatabaseAsync();

app.Run();

// Make Program class visible for testing
public partial class Program { }
