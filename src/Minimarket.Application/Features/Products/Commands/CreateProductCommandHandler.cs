using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Commands;
using Minimarket.Application.Features.Products.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Products.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Verificar si el código ya existe
        var existingProduct = (await _unitOfWork.Products.FindAsync(p => p.Code == request.Product.Code, cancellationToken))
            .FirstOrDefault();

        if (existingProduct != null)
        {
            return Result<ProductDto>.Failure("Ya existe un producto con este código");
        }

        // Verificar que la categoría existe
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Product.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result<ProductDto>.Failure("La categoría especificada no existe");
        }

        var product = new Product
        {
            Code = request.Product.Code,
            Name = request.Product.Name,
            Description = request.Product.Description,
            PurchasePrice = request.Product.PurchasePrice,
            SalePrice = request.Product.SalePrice,
            Stock = request.Product.Stock,
            MinimumStock = request.Product.MinimumStock,
            CategoryId = request.Product.CategoryId,
            ImageUrl = request.Product.ImageUrl,
            IsActive = true
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
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

