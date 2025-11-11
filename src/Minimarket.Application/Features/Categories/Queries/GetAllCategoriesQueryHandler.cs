using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.Queries;
using Minimarket.Application.Features.Categories.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Categories.Queries;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<IEnumerable<CategoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        
        // Obtener conteo de productos por categoría
        var categoryIds = categories.Select(c => c.Id).ToList();
        var productCounts = await _unitOfWork.ProductRepository.GetCountByCategoryIdsAsync(categoryIds, cancellationToken);

        // Debug: Verificar que se están obteniendo conteos
        // Esto se puede remover después de verificar que funciona
        if (productCounts.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"Conteos obtenidos: {string.Join(", ", productCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
        }

        var result = categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Orden)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IconoUrl = c.IconoUrl,
                Orden = c.Orden,
                IsActive = c.IsActive,
                ProductCount = productCounts.GetValueOrDefault(c.Id, 0)
            })
            .ToList();

        return Result<IEnumerable<CategoryDto>>.Success(result);
    }
}

