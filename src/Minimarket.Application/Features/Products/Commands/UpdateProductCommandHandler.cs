using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Commands;
using Minimarket.Application.Features.Products.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Products.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Product.Id, cancellationToken);

        if (product == null)
        {
            return Result<ProductDto>.Failure("Producto no encontrado");
        }

        // Verificar si el código ya existe en otro producto
        var existingProduct = (await _unitOfWork.Products.FindAsync(p => p.Code == request.Product.Code && p.Id != request.Product.Id, cancellationToken))
            .FirstOrDefault();

        if (existingProduct != null)
        {
            return Result<ProductDto>.Failure("Ya existe otro producto con este código");
        }

        // Verificar que la categoría existe
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Product.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result<ProductDto>.Failure("La categoría especificada no existe");
        }

        product.Code = request.Product.Code;
        product.Name = request.Product.Name;
        product.Description = request.Product.Description;
        product.PurchasePrice = request.Product.PurchasePrice;
        product.SalePrice = request.Product.SalePrice;
        product.Stock = request.Product.Stock;
        product.MinimumStock = request.Product.MinimumStock;
        product.CategoryId = request.Product.CategoryId;
        product.ImageUrl = request.Product.ImageUrl;
        product.IsActive = request.Product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            CategoryName = category.Name,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };

        return Result<ProductDto>.Success(result);
    }
}

