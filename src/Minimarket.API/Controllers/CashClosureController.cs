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
    private readonly ILogger<CashClosureController> _logger;

    public CashClosureController(IMediator mediator, IPdfService pdfService, ILogger<CashClosureController> logger)
    {
        _mediator = mediator;
        _pdfService = pdfService;
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
}

