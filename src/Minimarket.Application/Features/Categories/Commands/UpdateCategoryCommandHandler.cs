using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.Commands;
using Minimarket.Application.Features.Categories.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Categories.Commands;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating category {CategoryId}", request.Id);

        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Category not found. CategoryId: {CategoryId}", request.Id);
            throw new NotFoundException("Category", request.Id);
        }

        // Verificar si ya existe otra categoría con el mismo nombre
        var existingCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(
            c => c.Name.ToLower() == request.Category.Name.ToLower() && c.Id != request.Id,
            cancellationToken
        );

        if (existingCategory != null)
        {
            _logger.LogWarning("Attempted to update category with duplicate name {CategoryName}. Existing CategoryId: {ExistingCategoryId}", 
                request.Category.Name, existingCategory.Id);
            throw new BusinessRuleViolationException("Ya existe otra categoría con ese nombre");
        }

        category.Name = request.Category.Name;
        category.Description = request.Category.Description ?? string.Empty;
        category.ImageUrl = request.Category.ImageUrl;
        category.IconoUrl = request.Category.IconoUrl;
        category.Orden = request.Category.Orden;
        category.IsActive = request.Category.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category updated successfully. CategoryId: {CategoryId}, Name: {CategoryName}", 
            category.Id, category.Name);

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IconoUrl = category.IconoUrl,
            Orden = category.Orden,
            IsActive = category.IsActive
        };

        return Result<CategoryDto>.Success(categoryDto);
    }
}

