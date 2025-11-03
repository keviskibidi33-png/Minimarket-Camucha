using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Categories.Commands;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);

        if (category == null)
        {
            return Result<bool>.Failure("Categoría no encontrada");
        }

        // Verificar si hay productos asociados a esta categoría
        var hasProducts = await _unitOfWork.Products.ExistsAsync(
            p => p.CategoryId == request.Id && p.IsActive,
            cancellationToken
        );

        if (hasProducts)
        {
            // Soft delete: desactivar en lugar de eliminar
            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        }
        else
        {
            // Si no hay productos, eliminar físicamente
            await _unitOfWork.Categories.DeleteAsync(category, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

