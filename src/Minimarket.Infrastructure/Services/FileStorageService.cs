using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
        // Obtener la URL base desde configuración o usar BaseUrl del appsettings
        var configuredBaseUrl = _configuration["FileStorage:BaseUrl"] ?? _configuration["BaseUrl"];
        if (!string.IsNullOrEmpty(configuredBaseUrl))
        {
            _baseUrl = configuredBaseUrl;
        }
        else
        {
            // Fallback: usar localhost:5000 (puerto por defecto del backend)
            _baseUrl = "http://localhost:5000";
        }
        
        _logger.LogInformation("FileStorageService inicializado con BaseUrl: {BaseUrl}", _baseUrl);
        
        // Crear directorios si no existen
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
            _logger.LogInformation("Directorio de uploads creado: {UploadsPath}", _uploadsPath);
        }

        // Crear subdirectorios comunes
        var commonFolders = new[] { "products", "sedes", "payment-qr", "banners", "general" };
        foreach (var folder in commonFolders)
        {
            var folderPath = Path.Combine(_uploadsPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation("Directorio de carpeta creado: {FolderPath}", folderPath);
            }
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        try
        {
            // Validar extensión
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"Tipo de archivo no permitido. Solo se permiten: {string.Join(", ", allowedExtensions)}");
            }

            // Generar nombre único
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var folderPath = Path.Combine(_uploadsPath, folder);
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, uniqueFileName);

            // Guardar archivo
            using var fileStreamWrite = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamWrite);

            _logger.LogInformation("Archivo guardado: {FilePath}", filePath);

            // Retornar ruta relativa
            return Path.Combine("uploads", folder, uniqueFileName).Replace("\\", "/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar archivo: {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("Archivo eliminado: {FilePath}", fullPath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo: {FilePath}", filePath);
            return false;
        }
    }

    public string GetFileUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;

        // Si ya es una URL completa, retornarla
        if (filePath.StartsWith("http://") || filePath.StartsWith("https://"))
            return filePath;

        // Normalizar la ruta (remover barras iniciales duplicadas)
        var normalizedPath = filePath.Replace("\\", "/").TrimStart('/');

        // Si es una ruta de assets (logo, favicon, etc.), retornarla como relativa
        // El frontend servirá estos archivos desde /assets/
        if (normalizedPath.StartsWith("assets/"))
        {
            return normalizedPath; // Retornar como ruta relativa para que Nginx la sirva
        }

        // Para archivos subidos (uploads/), construir URL absoluta
        // Intentar obtener la URL base del contexto HTTP actual si está disponible
        string baseUrl = _baseUrl;
        if (_httpContextAccessor?.HttpContext != null)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            // Asegurar que siempre use HTTPS en producción
            var scheme = request.Scheme;
            // Si estamos en producción y el request viene por HTTP, forzar HTTPS
            if (request.Host.Host.Contains("edvio.app") || request.Host.Host.Contains("minimarket"))
            {
                scheme = "https";
            }
            baseUrl = $"{scheme}://{request.Host}";
        }
        else
        {
            // Si no hay contexto HTTP, usar BaseUrl de configuración
            // Asegurar HTTPS si la URL contiene dominios de producción
            if (baseUrl.Contains("edvio.app") || baseUrl.Contains("minimarket"))
            {
                baseUrl = baseUrl.Replace("http://", "https://");
            }
        }
        
        // Construir URL absoluta para archivos subidos
        var url = $"{baseUrl.TrimEnd('/')}/{normalizedPath}";
        return url;
    }
}

