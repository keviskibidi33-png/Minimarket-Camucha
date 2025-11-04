using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Products.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product {ProductId}", request.Id);

        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product not found. ProductId: {ProductId}", request.Id);
            throw new NotFoundException("Product", request.Id);
        }

        // Verificar si tiene ventas asociadas
        var hasSales = (await _unitOfWork.SaleDetails.FindAsync(sd => sd.ProductId == request.Id, cancellationToken))
            .Any();

        if (hasSales)
        {
            // En lugar de eliminar, desactivar el producto
            _logger.LogInformation("Product has sales, deactivating instead of deleting. ProductId: {ProductId}", request.Id);
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        }
        else
        {
            _logger.LogInformation("Product has no sales, deleting. ProductId: {ProductId}", request.Id);
            await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product deleted/deactivated successfully. ProductId: {ProductId}", request.Id);

        return Result<bool>.Success(true);
    }
}

