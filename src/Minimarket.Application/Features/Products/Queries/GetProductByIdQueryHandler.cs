using MediatR;
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
            return Result<ProductDto>.Failure("Producto no encontrado");
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
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };

        return Result<ProductDto>.Success(result);
    }
}

