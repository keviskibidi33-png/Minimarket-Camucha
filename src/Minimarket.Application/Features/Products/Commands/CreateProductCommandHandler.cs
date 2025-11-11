using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Commands;
using Minimarket.Application.Features.Products.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Products.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product with code {ProductCode}", request.Product.Code);

        // Verificar si el código ya existe
        var existingProduct = (await _unitOfWork.Products.FindAsync(p => p.Code == request.Product.Code, cancellationToken))
            .FirstOrDefault();

        if (existingProduct != null)
        {
            _logger.LogWarning("Attempted to create duplicate product with code {ProductCode}", request.Product.Code);
            throw new BusinessRuleViolationException("Ya existe un producto con este código");
        }

        // Verificar que la categoría existe
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Product.CategoryId, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category not found. CategoryId: {CategoryId}", request.Product.CategoryId);
            throw new NotFoundException("Category", request.Product.CategoryId);
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
            IsActive = true,
            ExpirationDate = request.Product.ExpirationDate
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created successfully. ProductId: {ProductId}, Code: {ProductCode}", 
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

