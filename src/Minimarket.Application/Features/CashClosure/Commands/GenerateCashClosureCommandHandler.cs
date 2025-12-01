using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.CashClosure.Commands;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.CashClosure.Commands;

public class GenerateCashClosureCommandHandler : IRequestHandler<GenerateCashClosureCommand, Result<GenerateCashClosureResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateCashClosureCommandHandler> _logger;

    public GenerateCashClosureCommandHandler(IUnitOfWork unitOfWork, ILogger<GenerateCashClosureCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<GenerateCashClosureResponse>> Handle(GenerateCashClosureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo ventas para cierre de caja desde {StartDate} hasta {EndDate}", 
                request.StartDate, request.EndDate);

            // Obtener todas las ventas pagadas del período que no estén cerradas
            var (sales, _) = await _unitOfWork.SaleRepository.GetPagedSalesAsync(
                startDate: request.StartDate,
                endDate: request.EndDate,
                customerId: null,
                userId: null,
                documentNumber: null,
                page: 1,
                pageSize: 100000, // Obtener todas
                cancellationToken: cancellationToken
            );

            // Filtrar solo ventas pagadas que no estén cerradas
            var paidSales = sales
                .Where(s => s.Status == SaleStatus.Pagado && !s.IsClosed)
                .ToList();

            var totalPaid = paidSales.Sum(s => s.Total);
            var totalCount = paidSales.Count;

            _logger.LogInformation("Encontradas {Count} ventas pagadas con total de {Total}", totalCount, totalPaid);

            return Result<GenerateCashClosureResponse>.Success(new GenerateCashClosureResponse
            {
                Sales = paidSales,
                TotalPaid = totalPaid,
                TotalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo ventas para cierre de caja");
            return Result<GenerateCashClosureResponse>.Failure($"Error al obtener ventas: {ex.Message}");
        }
    }
}

