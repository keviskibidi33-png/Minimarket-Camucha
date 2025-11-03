using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Commands;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sales.Commands;

public class CancelSaleCommandHandler : IRequestHandler<CancelSaleCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelSaleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(request.SaleId, cancellationToken);

            if (sale == null)
            {
                return Result<bool>.Failure("Venta no encontrada");
            }

            if (sale.Status == SaleStatus.Anulado)
            {
                return Result<bool>.Failure("La venta ya está anulada");
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

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<bool>.Failure($"Error al anular la venta: {ex.Message}");
        }
    }
}

