using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Domain.Interfaces;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileStorageService fileStorageService, ILogger<FilesController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)] // 10MB
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    [Authorize(Roles = "Administrador,Almacenero")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "products")
    {
        try
        {
            _logger.LogInformation("Iniciando subida de archivo. Folder: {Folder}, FileName: {FileName}, Size: {Size}", 
                folder, file?.FileName, file?.Length);

            // Validar archivo
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Intento de subir archivo nulo o vacío");
                return BadRequest(new { error = "No se proporcionó ningún archivo" });
            }

            // Validar tamaño (10MB máximo - debe coincidir con RequestFormLimits y RequestSizeLimit)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize)
            {
                _logger.LogWarning("Archivo excede tamaño máximo. Tamaño: {Size} bytes, Máximo: {MaxSize} bytes", 
                    file.Length, maxFileSize);
                return BadRequest(new { error = "El archivo excede el tamaño máximo de 10MB" });
            }

            // Validar tipo de archivo
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Tipo de archivo no permitido. Extensión: {Extension}, Permitidas: {Allowed}", 
                    extension, string.Join(", ", allowedExtensions));
                return BadRequest(new { error = $"Tipo de archivo no permitido. Solo se permiten: {string.Join(", ", allowedExtensions)}" });
            }

            // Normalizar nombre de carpeta (permitir cualquier carpeta válida)
            var normalizedFolder = folder?.Trim().ToLowerInvariant() ?? "products";
            // Permitir: products, sedes, payment-qr, banners, general, etc.
            // Solo validar que no tenga caracteres peligrosos
            if (normalizedFolder.Contains("..") || normalizedFolder.Contains("/") || normalizedFolder.Contains("\\"))
            {
                _logger.LogWarning("Nombre de carpeta inválido: {Folder}", folder);
                return BadRequest(new { error = "Nombre de carpeta inválido" });
            }

            _logger.LogInformation("Guardando archivo en carpeta: {Folder}", normalizedFolder);

            // Guardar archivo
            using var stream = file.OpenReadStream();
            var filePath = await _fileStorageService.SaveFileAsync(stream, file.FileName, normalizedFolder);
            var fileUrl = _fileStorageService.GetFileUrl(filePath);

            _logger.LogInformation("Archivo guardado exitosamente. FilePath: {FilePath}, FileUrl: {FileUrl}", 
                filePath, fileUrl);

            return Ok(new { 
                filePath, 
                fileUrl,
                url = fileUrl, // Alias para compatibilidad con frontend
                fileName = file.FileName,
                size = file.Length 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo. Folder: {Folder}, FileName: {FileName}", 
                folder, file?.FileName);
            return StatusCode(500, new { error = "Error al procesar el archivo: " + ex.Message });
        }
    }

    [HttpDelete("{*filePath}")]
    [Authorize(Roles = "Administrador,Almacenero")]
    public async Task<IActionResult> Delete(string filePath)
    {
        try
        {
            var result = await _fileStorageService.DeleteFileAsync(filePath);
            
            if (!result)
            {
                return NotFound(new { error = "Archivo no encontrado" });
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo: {FilePath}", filePath);
            return StatusCode(500, new { error = "Error al eliminar el archivo" });
        }
    }
}

