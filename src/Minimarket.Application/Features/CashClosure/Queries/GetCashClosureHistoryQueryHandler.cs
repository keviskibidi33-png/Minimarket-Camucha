using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.CashClosure.Queries;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.CashClosure.Queries;

public class GetCashClosureHistoryQueryHandler : IRequestHandler<GetCashClosureHistoryQuery, Result<List<CashClosureHistoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetCashClosureHistoryQueryHandler> _logger;

    public GetCashClosureHistoryQueryHandler(IUnitOfWork unitOfWork, ILogger<GetCashClosureHistoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<CashClosureHistoryDto>>> Handle(GetCashClosureHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo historial de cierres de caja desde {StartDate} hasta {EndDate}",
                request.StartDate, request.EndDate);

            // Obtener todas las ventas cerradas usando FindAsync para filtrar directamente en la base de datos
            var allClosedSales = await _unitOfWork.Sales.FindAsync(
                s => s.Status == SaleStatus.Pagado && s.IsClosed && s.CashClosureDate.HasValue,
                cancellationToken
            );

            var closedSales = allClosedSales.ToList();

            // Aplicar filtros de fecha por CashClosureDate si se proporcionan
            if (request.StartDate.HasValue)
            {
                closedSales = closedSales
                    .Where(s => s.CashClosureDate!.Value.Date >= request.StartDate.Value.Date)
                    .ToList();
            }

            if (request.EndDate.HasValue)
            {
                var endDate = request.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                closedSales = closedSales
                    .Where(s => s.CashClosureDate!.Value.Date <= endDate.Date)
                    .ToList();
            }

            // Agrupar por fecha de cierre
            var closures = closedSales
                .GroupBy(s => s.CashClosureDate!.Value.Date)
                .Select(g => new CashClosureHistoryDto
                {
                    ClosureDate = g.Key,
                    SalesStartDate = g.Min(s => s.SaleDate),
                    SalesEndDate = g.Max(s => s.SaleDate),
                    TotalSales = g.Count(),
                    TotalAmount = g.Sum(s => s.Total),
                    ByPaymentMethod = g
                        .GroupBy(s => s.PaymentMethod)
                        .Select(pg => new PaymentMethodSummaryDto
                        {
                            Method = pg.Key.ToString(),
                            Count = pg.Count(),
                            Total = pg.Sum(s => s.Total)
                        })
                        .OrderByDescending(x => x.Total)
                        .ToList()
                })
                .OrderByDescending(c => c.ClosureDate)
                .ToList();

            _logger.LogInformation("Encontrados {Count} cierres de caja en el historial", closures.Count);

            return Result<List<CashClosureHistoryDto>>.Success(closures);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo historial de cierres de caja");
            return Result<List<CashClosureHistoryDto>>.Failure($"Error al obtener historial: {ex.Message}");
        }
    }
}

