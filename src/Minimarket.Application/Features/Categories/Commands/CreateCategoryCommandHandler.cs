using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.Commands;
using Minimarket.Application.Features.Categories.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Categories.Commands;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating category with name {CategoryName}", request.Category.Name);

        // Verificar si ya existe una categoría con el mismo nombre
        var existingCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(
            c => c.Name.ToLower() == request.Category.Name.ToLower(),
            cancellationToken
        );

        if (existingCategory != null)
        {
            _logger.LogWarning("Attempted to create duplicate category with name {CategoryName}", request.Category.Name);
            throw new BusinessRuleViolationException("Ya existe una categoría con ese nombre");
        }

        var category = new Category
        {
            Name = request.Category.Name,
            Description = request.Category.Description ?? string.Empty,
            ImageUrl = request.Category.ImageUrl,
            IconoUrl = request.Category.IconoUrl,
            Orden = request.Category.Orden,
            IsActive = true
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category created successfully. CategoryId: {CategoryId}, Name: {CategoryName}", 
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

