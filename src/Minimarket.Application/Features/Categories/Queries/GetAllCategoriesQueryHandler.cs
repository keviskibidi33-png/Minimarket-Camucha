using MediatR;
using System.Linq.Expressions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.Queries;
using Minimarket.Application.Features.Categories.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Categories.Queries;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<PagedResult<CategoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        // Construir predicado de búsqueda si hay término de búsqueda
        Expression<Func<Domain.Entities.Category, bool>>? predicate = null;
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower().Trim();
            predicate = c => c.IsActive && 
                (c.Name.ToLower().Contains(searchTerm) || 
                 (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
        }
        else
        {
            predicate = c => c.IsActive;
        }

        // Obtener categorías paginadas
        var pagedCategories = await _unitOfWork.CategoryRepository.GetPagedAsync(
            pageNumber: request.Page,
            pageSize: request.PageSize,
            predicate: predicate,
            orderBy: c => c.Orden,
            ascending: true,
            cancellationToken: cancellationToken
        );

        // Obtener conteo de productos por categoría
        var categoryIds = pagedCategories.Items.Select(c => c.Id).ToList();
        var productCounts = await _unitOfWork.ProductRepository.GetCountByCategoryIdsAsync(categoryIds, cancellationToken);

        // Mapear a DTOs
        var categoryDtos = pagedCategories.Items.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            IconoUrl = c.IconoUrl,
            Orden = c.Orden,
            IsActive = c.IsActive,
            ProductCount = productCounts.GetValueOrDefault(c.Id, 0)
        }).ToList();

        var result = PagedResult<CategoryDto>.Create(
            categoryDtos,
            pagedCategories.TotalCount,
            request.Page,
            request.PageSize
        );

        return Result<PagedResult<CategoryDto>>.Success(result);
    }
}

