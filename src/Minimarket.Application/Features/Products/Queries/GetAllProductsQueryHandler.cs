using MediatR;
using Microsoft.EntityFrameworkCore;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Queries;
using Minimarket.Application.Features.Products.DTOs;
using Minimarket.Domain.Interfaces;
using Minimarket.Infrastructure.Data;

namespace Minimarket.Application.Features.Products.Queries;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly MinimarketDbContext _context;

    public GetAllProductsQueryHandler(MinimarketDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // Construir query en base de datos (IQueryable)
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchLower) || 
                p.Code.ToLower().Contains(searchLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower)));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        // Contar total antes de paginación
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginación en base de datos
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                PurchasePrice = p.PurchasePrice,
                SalePrice = p.SalePrice,
                Stock = p.Stock,
                MinimumStock = p.MinimumStock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Sin categoría",
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<ProductDto>.Create(products, totalCount, request.Page, request.PageSize);

        return Result<PagedResult<ProductDto>>.Success(pagedResult);
    }
}

