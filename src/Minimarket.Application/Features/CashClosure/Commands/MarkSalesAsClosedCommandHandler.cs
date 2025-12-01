using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.CashClosure.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.CashClosure.Commands;

public class MarkSalesAsClosedCommandHandler : IRequestHandler<MarkSalesAsClosedCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkSalesAsClosedCommandHandler> _logger;

    public MarkSalesAsClosedCommandHandler(IUnitOfWork unitOfWork, ILogger<MarkSalesAsClosedCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(MarkSalesAsClosedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Marcando {Count} ventas como cerradas", request.SaleIds.Count);

            var closureDate = DateTime.UtcNow;

            foreach (var saleId in request.SaleIds)
            {
                var sale = await _unitOfWork.Sales.GetByIdAsync(saleId, cancellationToken);
                if (sale != null)
                {
                    sale.IsClosed = true;
                    sale.CashClosureDate = closureDate;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Ventas marcadas como cerradas exitosamente");

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marcando ventas como cerradas");
            return Result<bool>.Failure($"Error al marcar ventas como cerradas: {ex.Message}");
        }
    }
}

