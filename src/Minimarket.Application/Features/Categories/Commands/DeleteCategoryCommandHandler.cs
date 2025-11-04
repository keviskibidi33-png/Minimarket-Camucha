using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Categories.Commands;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting category {CategoryId}", request.Id);

        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Category not found. CategoryId: {CategoryId}", request.Id);
            throw new NotFoundException("Category", request.Id);
        }

        // Verificar si hay productos asociados a esta categoría
        var hasProducts = await _unitOfWork.Products.ExistsAsync(
            p => p.CategoryId == request.Id && p.IsActive,
            cancellationToken
        );

        if (hasProducts)
        {
            // Soft delete: desactivar en lugar de eliminar
            _logger.LogInformation("Category has products, deactivating instead of deleting. CategoryId: {CategoryId}", request.Id);
            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        }
        else
        {
            // Si no hay productos, eliminar físicamente
            _logger.LogInformation("Category has no products, deleting. CategoryId: {CategoryId}", request.Id);
            await _unitOfWork.Categories.DeleteAsync(category, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category deleted/deactivated successfully. CategoryId: {CategoryId}", request.Id);

        return Result<bool>.Success(true);
    }
}

