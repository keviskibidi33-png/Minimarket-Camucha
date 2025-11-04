using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = _configuration["FileStorage:BaseUrl"] ?? "https://localhost:5001";
        
        // Crear directorios si no existen
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }

        var productsPath = Path.Combine(_uploadsPath, "products");
        if (!Directory.Exists(productsPath))
        {
            Directory.CreateDirectory(productsPath);
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

        // Construir URL relativa
        return $"{_baseUrl}/{filePath.Replace("\\", "/")}";
    }
}

