using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Commands;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sales.Commands;

public class CancelSaleCommandHandler : IRequestHandler<CancelSaleCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelSaleCommandHandler> _logger;

    public CancelSaleCommandHandler(IUnitOfWork unitOfWork, ILogger<CancelSaleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling sale {SaleId} by user {UserId}", request.SaleId, request.UserId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(request.SaleId, cancellationToken);

            if (sale == null)
            {
                _logger.LogWarning("Sale not found. SaleId: {SaleId}", request.SaleId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw new NotFoundException("Sale", request.SaleId);
            }

            if (sale.Status == SaleStatus.Anulado)
            {
                _logger.LogWarning("Attempted to cancel already cancelled sale. SaleId: {SaleId}", request.SaleId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw new BusinessRuleViolationException("La venta ya está anulada");
            }

            // Restaurar stock de productos
            var saleDetails = (await _unitOfWork.SaleDetails.FindAsync(sd => sd.SaleId == sale.Id, cancellationToken)).ToList();

            foreach (var detail in saleDetails)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId, cancellationToken);
                if (product != null)
                {
                    product.Stock += detail.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
                }
            }

            // Anular la venta
            sale.Status = SaleStatus.Anulado;
            sale.CancellationReason = request.Reason;
            sale.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Sales.UpdateAsync(sale, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Sale cancelled successfully. SaleId: {SaleId}, DocumentNumber: {DocumentNumber}", 
                sale.Id, sale.DocumentNumber);

            return Result<bool>.Success(true);
        }
        catch (NotFoundException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogWarning(ex, "Sale not found during cancellation");
            throw;
        }
        catch (BusinessRuleViolationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogWarning(ex, "Business rule violation during sale cancellation");
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Unexpected error cancelling sale {SaleId}", request.SaleId);
            throw;
        }
    }
}

