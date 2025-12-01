using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.DocumentSettings.Commands;
using Minimarket.Application.Features.DocumentSettings.Queries;
using Minimarket.Domain.Interfaces;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/document-settings")]
public class DocumentSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("view-settings")]
    [AllowAnonymous] // Permitir acceso público para obtener configuración
    public async Task<IActionResult> GetViewSettings()
    {
        try
        {
            var query = new GetDocumentViewSettingsQuery();
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                // Si hay un error, devolver valores por defecto en lugar de un error
                return Ok(new
                {
                    defaultViewMode = "preview",
                    directPrint = false,
                    boletaTemplateActive = true,
                    facturaTemplateActive = true
                });
            }

            return Ok(result.Data);
        }
        catch (Exception)
        {
            // En caso de excepción, devolver valores por defecto
            return Ok(new
            {
                defaultViewMode = "preview",
                directPrint = false,
                boletaTemplateActive = true,
                facturaTemplateActive = true
            });
        }
    }

    [HttpPut("view-settings")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateViewSettings([FromBody] UpdateDocumentViewSettingsCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("preview-pdf")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetPreviewPdf([FromBody] PreviewPdfRequest request)
    {
        try
        {
            var pdfService = HttpContext.RequestServices.GetRequiredService<IPdfService>();
            
            var documentType = request.DocumentType ?? "Boleta";
            
            // Validar que el tipo de documento sea válido
            if (documentType != "Boleta" && documentType != "Factura")
            {
                return BadRequest(new { message = "Tipo de documento inválido. Debe ser 'Boleta' o 'Factura'" });
            }

            // Convertir el objeto de configuración a Dictionary
            Dictionary<string, string>? customSettings = null;
            if (request.Settings != null)
            {
                customSettings = new Dictionary<string, string>();
                
                // Agregar los valores del formulario
                if (request.Settings.CompanyName != null)
                    customSettings["companyName"] = request.Settings.CompanyName;
                if (request.Settings.CompanyRuc != null)
                    customSettings["companyRuc"] = request.Settings.CompanyRuc;
                if (request.Settings.CompanyAddress != null)
                    customSettings["companyAddress"] = request.Settings.CompanyAddress;
                if (request.Settings.CompanyPhone != null)
                    customSettings["companyPhone"] = request.Settings.CompanyPhone;
                if (request.Settings.CompanyEmail != null)
                    customSettings["companyEmail"] = request.Settings.CompanyEmail;
                if (request.Settings.LogoUrl != null)
                    customSettings["logoUrl"] = request.Settings.LogoUrl;
            }

            // Generar PDF de prueba con las configuraciones del formulario o BrandSettings
            var pdfPath = await pdfService.GeneratePreviewPdfAsync(documentType, customSettings);

            if (!System.IO.File.Exists(pdfPath))
            {
                return NotFound(new { message = "El documento PDF de prueba no pudo ser generado" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
            var fileName = Path.GetFileName(pdfPath);

            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar el documento PDF de prueba", error = ex.Message });
        }
    }
}

public class PreviewPdfRequest
{
    public string? DocumentType { get; set; }
    public PreviewPdfSettings? Settings { get; set; }
}

public class PreviewPdfSettings
{
    public string? CompanyName { get; set; }
    public string? CompanyRuc { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
    public string? LogoUrl { get; set; }
}
