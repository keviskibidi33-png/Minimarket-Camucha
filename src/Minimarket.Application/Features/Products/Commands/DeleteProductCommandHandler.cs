using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Products.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            return Result<bool>.Failure("Producto no encontrado");
        }

        // Verificar si tiene ventas asociadas
        var hasSales = (await _unitOfWork.SaleDetails.FindAsync(sd => sd.ProductId == request.Id, cancellationToken))
            .Any();

        if (hasSales)
        {
            // En lugar de eliminar, desactivar el producto
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        }
        else
        {
            await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

