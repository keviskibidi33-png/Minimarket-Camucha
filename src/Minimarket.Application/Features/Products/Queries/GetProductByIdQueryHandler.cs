using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Queries;
using Minimarket.Application.Features.Products.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Products.Queries;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Product", request.Id);
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId, cancellationToken);

        var result = new ProductDto
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            PurchasePrice = product.PurchasePrice,
            SalePrice = product.SalePrice,
            Stock = product.Stock,
            MinimumStock = product.MinimumStock,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name ?? "Sin categoría",
            ImageUrl = product.ImageUrl,
            Imagenes = ParseJsonArray(product.ImagenesJson),
            Paginas = ParseJsonObject(product.PaginasJson),
            SedesDisponibles = ParseJsonGuidArray(product.SedesDisponiblesJson),
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };

        return Result<ProductDto>.Success(result);
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

