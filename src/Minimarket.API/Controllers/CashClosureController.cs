using System.IO;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Features.CashClosure.Commands;
using Minimarket.Application.Features.CashClosure.Queries;
using Minimarket.Domain.Interfaces;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/cash-closure")]
[Authorize(Roles = "Administrador,Cajero")]
public class CashClosureController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPdfService _pdfService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CashClosureController> _logger;

    public CashClosureController(IMediator mediator, IPdfService pdfService, IUnitOfWork unitOfWork, ILogger<CashClosureController> logger)
    {
        _mediator = mediator;
        _pdfService = pdfService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateCashClosure([FromBody] GenerateCashClosureCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        // Generar PDF
        try
        {
            var pdfPath = await _pdfService.GenerateCashClosurePdfAsync(
                command.StartDate,
                command.EndDate,
                result.Data!.Sales
            );

            if (!System.IO.File.Exists(pdfPath))
            {
                return StatusCode(500, new { message = "Error al generar el PDF" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
            var fileName = Path.GetFileName(pdfPath);

            // Marcar ventas como cerradas
            var markClosedResult = await _mediator.Send(new MarkSalesAsClosedCommand
            {
                SaleIds = result.Data!.Sales.Select(s => s.Id).ToList()
            });

            if (!markClosedResult.Succeeded)
            {
                _logger.LogWarning("PDF generado pero no se pudieron marcar las ventas como cerradas");
            }

            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar el PDF", error = ex.Message });
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = new GetCashClosureSummaryQuery
        {
            StartDate = startDate ?? DateTime.Today,
            EndDate = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1)
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpGet("history")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetHistory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = new GetCashClosureHistoryQuery
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpGet("download-pdf")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DownloadCashClosurePdf([FromQuery] DateTime closureDate)
    {
        try
        {
            _logger.LogInformation("Generando PDF de cierre de caja para la fecha {ClosureDate}", closureDate);

            // Obtener todas las ventas cerradas en esa fecha específica
            var startOfDay = closureDate.Date;
            var endOfDay = closureDate.Date.AddDays(1).AddSeconds(-1);

            // Obtener ventas cerradas usando GetPagedSalesAsync para incluir detalles
            var (allSales, _) = await _unitOfWork.SaleRepository.GetPagedSalesAsync(
                startDate: closureDate.Date.AddDays(-30), // Rango amplio para asegurar que encontremos las ventas
                endDate: closureDate.Date.AddDays(1),
                customerId: null,
                userId: null,
                documentNumber: null,
                page: 1,
                pageSize: 100000, // Obtener todas
                cancellationToken: default
            );

            var closedSales = allSales
                .Where(s => s.Status == Domain.Enums.SaleStatus.Pagado && 
                           s.IsClosed && 
                           s.CashClosureDate.HasValue &&
                           s.CashClosureDate.Value.Date == closureDate.Date)
                .ToList();

            if (!closedSales.Any())
            {
                return NotFound(new { message = "No se encontraron ventas cerradas para la fecha especificada" });
            }

            // Generar PDF
            var pdfPath = await _pdfService.GenerateCashClosurePdfAsync(
                startOfDay,
                endOfDay,
                closedSales
            );

            if (!System.IO.File.Exists(pdfPath))
            {
                return StatusCode(500, new { message = "Error al generar el PDF" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
            var fileName = $"Cierre_Caja_{closureDate:yyyy-MM-dd}.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando PDF de cierre de caja para fecha {ClosureDate}", closureDate);
            return StatusCode(500, new { message = "Error al generar el PDF", error = ex.Message });
        }
    }
}

