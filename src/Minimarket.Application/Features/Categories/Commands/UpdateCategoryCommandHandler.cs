using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.Commands;
using Minimarket.Application.Features.Categories.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Categories.Commands;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);

        if (category == null)
        {
            return Result<CategoryDto>.Failure("Categoría no encontrada");
        }

        // Verificar si ya existe otra categoría con el mismo nombre
        var existingCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(
            c => c.Name.ToLower() == request.Category.Name.ToLower() && c.Id != request.Id,
            cancellationToken
        );

        if (existingCategory != null)
        {
            return Result<CategoryDto>.Failure("Ya existe otra categoría con ese nombre");
        }

        category.Name = request.Category.Name;
        category.Description = request.Category.Description ?? string.Empty;
        category.IsActive = request.Category.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        };

        return Result<CategoryDto>.Success(categoryDto);
    }
}

