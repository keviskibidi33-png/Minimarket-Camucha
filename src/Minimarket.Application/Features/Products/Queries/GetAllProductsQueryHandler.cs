using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Queries;
using Minimarket.Application.Features.Products.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Products.Queries;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // Obtener todos los productos
        var allProducts = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        
        // Obtener todas las categorías para mapear nombres
        var allCategories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        var categoriesDict = allCategories.ToDictionary(c => c.Id, c => c.Name);

        // Aplicar filtros en memoria
        var filteredProducts = allProducts.AsEnumerable();

        // Aplicar filtro de búsqueda (case-insensitive)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.Trim().ToLowerInvariant();
            filteredProducts = filteredProducts.Where(p => 
                (p.Name != null && p.Name.ToLowerInvariant().Contains(searchLower)) || 
                (p.Code != null && p.Code.ToLowerInvariant().Contains(searchLower)) ||
                (p.Description != null && p.Description.ToLowerInvariant().Contains(searchLower)));
        }

        // Aplicar filtro de categoría
        if (request.CategoryId.HasValue)
        {
            filteredProducts = filteredProducts.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        // Aplicar filtro de estado activo (por defecto, si no se especifica, mostrar todos)
        if (request.IsActive.HasValue)
        {
            filteredProducts = filteredProducts.Where(p => p.IsActive == request.IsActive.Value);
        }

        // Ordenar por nombre
        var sortedProducts = filteredProducts.OrderBy(p => p.Name ?? string.Empty).ToList();

        // Contar total antes de paginación
        var totalCount = sortedProducts.Count;

        // Validar página
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(100, request.PageSize)); // Limitar a máximo 100 items por página

        // Aplicar paginación
        var products = sortedProducts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Code = p.Code ?? string.Empty,
                Name = p.Name ?? string.Empty,
                Description = p.Description,
                PurchasePrice = p.PurchasePrice,
                SalePrice = p.SalePrice,
                Stock = p.Stock,
                MinimumStock = p.MinimumStock,
                CategoryId = p.CategoryId,
                CategoryName = categoriesDict.TryGetValue(p.CategoryId, out var categoryName) ? categoryName : "Sin categoría",
                ImageUrl = p.ImageUrl,
                Imagenes = ParseJsonArray(p.ImagenesJson),
                Paginas = ParseJsonObject(p.PaginasJson),
                SedesDisponibles = ParseJsonGuidArray(p.SedesDisponiblesJson),
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                ExpirationDate = p.ExpirationDate
            })
            .ToList();

        var pagedResult = PagedResult<ProductDto>.Create(products, totalCount, page, pageSize);

        return Result<PagedResult<ProductDto>>.Success(pagedResult);
    }

    private static List<string> ParseJsonArray(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<string>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static Dictionary<string, object> ParseJsonObject(string json)
    {
        if (string.IsNullOrEmpty(json)) return new Dictionary<string, object>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private static List<Guid> ParseJsonGuidArray(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<Guid>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }
}

