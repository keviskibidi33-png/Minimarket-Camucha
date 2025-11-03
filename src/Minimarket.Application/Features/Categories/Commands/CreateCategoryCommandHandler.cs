using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.Commands;
using Minimarket.Application.Features.Categories.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Categories.Commands;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Verificar si ya existe una categoría con el mismo nombre
        var existingCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(
            c => c.Name.ToLower() == request.Category.Name.ToLower(),
            cancellationToken
        );

        if (existingCategory != null)
        {
            return Result<CategoryDto>.Failure("Ya existe una categoría con ese nombre");
        }

        var category = new Category
        {
            Name = request.Category.Name,
            Description = request.Category.Description ?? string.Empty,
            IsActive = true
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
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

