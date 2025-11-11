using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Commands;
using Minimarket.Application.Features.Products.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Products.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product {ProductId}", request.Product.Id);

        var product = await _unitOfWork.Products.GetByIdAsync(request.Product.Id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product not found. ProductId: {ProductId}", request.Product.Id);
            throw new NotFoundException("Product", request.Product.Id);
        }

        // Verificar si el código ya existe en otro producto
        var existingProduct = (await _unitOfWork.Products.FindAsync(p => p.Code == request.Product.Code && p.Id != request.Product.Id, cancellationToken))
            .FirstOrDefault();

        if (existingProduct != null)
        {
            _logger.LogWarning("Attempted to update product with duplicate code {ProductCode}. Existing ProductId: {ExistingProductId}", 
                request.Product.Code, existingProduct.Id);
            throw new BusinessRuleViolationException("Ya existe otro producto con este código");
        }

        // Verificar que la categoría existe
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Product.CategoryId, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category not found. CategoryId: {CategoryId}", request.Product.CategoryId);
            throw new NotFoundException("Category", request.Product.CategoryId);
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
        product.ExpirationDate = request.Product.ExpirationDate;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product updated successfully. ProductId: {ProductId}, Code: {ProductCode}", 
            product.Id, product.Code);

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
            CreatedAt = product.CreatedAt,
            ExpirationDate = product.ExpirationDate
        };

        return Result<ProductDto>.Success(result);
    }
}

