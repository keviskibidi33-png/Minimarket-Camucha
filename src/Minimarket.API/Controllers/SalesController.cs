using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Features.Sales.Commands;
using Minimarket.Application.Features.Sales.Queries;
using Minimarket.Domain.Interfaces;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Cajero")]
public class SalesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleCommand command)
    {
        // Obtener el ID del usuario desde el token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized("Usuario no autenticado");
        }

        command.UserId = userId;
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllSalesQuery query)
    {
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSaleByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelSaleRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized("Usuario no autenticado");
        }

        var command = new CancelSaleCommand
        {
            SaleId = id,
            Reason = request.Reason,
            UserId = userId
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(new { success = true });
    }

    [HttpPost("{id}/send-receipt")]
    public async Task<IActionResult> SendReceipt(Guid id, [FromBody] SendReceiptRequest request)
    {
        var command = new SendSaleReceiptCommand
        {
            SaleId = id,
            Email = request.Email,
            DocumentType = request.DocumentType
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            // Retornar un objeto anónimo en lugar del Result directamente para evitar problemas de serialización
            return BadRequest(new 
            { 
                succeeded = false,
                errors = result.Errors,
                message = result.Errors.Length > 0 ? result.Errors[0] : "Error al enviar el comprobante"
            });
        }

        return Ok(new { success = true, message = "Comprobante enviado exitosamente" });
    }

    [HttpGet("{id}/pdf")]
    [AllowAnonymous] // Permitir acceso público para visualización de documentos
    public async Task<IActionResult> GetPdf(Guid id, [FromQuery] string documentType = "Boleta")
    {
        try
        {
            var pdfService = HttpContext.RequestServices.GetRequiredService<IPdfService>();
            
            // Validar que el tipo de documento sea válido
            if (documentType != "Boleta" && documentType != "Factura")
            {
                return BadRequest(new { message = "Tipo de documento inválido. Debe ser 'Boleta' o 'Factura'" });
            }

            // Validar que la plantilla esté disponible (validación interna)
            var pdfPath = await pdfService.GenerateSaleReceiptAsync(id, documentType);

            if (!System.IO.File.Exists(pdfPath))
            {
                return NotFound(new { message = "El documento PDF no pudo ser generado" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
            var fileName = Path.GetFileName(pdfPath);

            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar el documento PDF", error = ex.Message });
        }
    }
}

public class CancelSaleRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class SendReceiptRequest
{
    public string Email { get; set; } = string.Empty;
    public string DocumentType { get; set; } = "Boleta";
}

