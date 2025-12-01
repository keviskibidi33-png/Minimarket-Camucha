using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.CashClosure.Queries;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.CashClosure.Queries;

public class GetCashClosureSummaryQueryHandler : IRequestHandler<GetCashClosureSummaryQuery, Result<CashClosureSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetCashClosureSummaryQueryHandler> _logger;

    public GetCashClosureSummaryQueryHandler(IUnitOfWork unitOfWork, ILogger<GetCashClosureSummaryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CashClosureSummaryDto>> Handle(GetCashClosureSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo resumen de cierre de caja desde {StartDate} hasta {EndDate}", 
                request.StartDate, request.EndDate);

            var (sales, _) = await _unitOfWork.SaleRepository.GetPagedSalesAsync(
                startDate: request.StartDate,
                endDate: request.EndDate,
                customerId: null,
                userId: null,
                documentNumber: null,
                page: 1,
                pageSize: 100000,
                cancellationToken: cancellationToken
            );

            // Filtrar solo ventas pagadas que no estén cerradas
            var paidSales = sales
                .Where(s => s.Status == SaleStatus.Pagado && !s.IsClosed)
                .ToList();

            var totalPaid = paidSales.Sum(s => s.Total);
            var totalCount = paidSales.Count;

            var paymentGroups = paidSales
                .GroupBy(s => s.PaymentMethod)
                .Select(g => new PaymentMethodSummaryDto
                {
                    Method = g.Key.ToString(),
                    Count = g.Count(),
                    Total = g.Sum(s => s.Total)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            return Result<CashClosureSummaryDto>.Success(new CashClosureSummaryDto
            {
                TotalPaid = totalPaid,
                TotalCount = totalCount,
                ByPaymentMethod = paymentGroups
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo resumen de cierre de caja");
            return Result<CashClosureSummaryDto>.Failure($"Error al obtener resumen: {ex.Message}");
        }
    }
}

