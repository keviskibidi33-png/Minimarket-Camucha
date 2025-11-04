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
    [Authorize(Roles = "Administrador,Almacenero")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "products")
    {
        try
        {
            // Validar archivo
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No se proporcionó ningún archivo" });
            }

            // Validar tamaño (5MB máximo)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { error = "El archivo excede el tamaño máximo de 5MB" });
            }

            // Validar tipo de archivo
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { error = $"Tipo de archivo no permitido. Solo se permiten: {string.Join(", ", allowedExtensions)}" });
            }

            // Guardar archivo
            using var stream = file.OpenReadStream();
            var filePath = await _fileStorageService.SaveFileAsync(stream, file.FileName, folder);
            var fileUrl = _fileStorageService.GetFileUrl(filePath);

            return Ok(new { 
                filePath, 
                fileUrl,
                fileName = file.FileName,
                size = file.Length 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo");
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

