using FluentAssertions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Commands;
using Minimarket.Application.Features.Products.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;
using Moq;
using Xunit;

namespace Minimarket.UnitTests.Application.Commands;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateProductCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var existingProduct = new Product
        {
            Id = productId,
            Code = "7750001001001",
            Name = "Producto Original",
            SalePrice = 10.00m,
            CategoryId = categoryId,
            IsActive = true
        };

        var category = new Category
        {
            Id = categoryId,
            Name = "Bebidas",
            IsActive = true
        };

        var command = new UpdateProductCommand
        {
            Product = new UpdateProductDto
            {
                Id = productId,
                Code = "7750001001001",
                Name = "Producto Actualizado",
                Description = "Nueva descripción",
                PurchasePrice = 5.00m,
                SalePrice = 12.00m,
                Stock = 50,
                MinimumStock = 10,
                CategoryId = categoryId,
                IsActive = true
            }
        };

        _unitOfWorkMock
            .Setup(x => x.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>()); // No hay productos con código duplicado

        _unitOfWorkMock
            .Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _unitOfWorkMock
            .Setup(x => x.Products.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Producto Actualizado");
        result.Data.SalePrice.Should().Be(12.00m);

        _unitOfWorkMock.Verify(x => x.Products.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var command = new UpdateProductCommand
        {
            Product = new UpdateProductDto
            {
                Id = productId,
                Code = "7750001001001",
                Name = "Producto Actualizado",
                CategoryId = categoryId
            }
        };

        _unitOfWorkMock
            .Setup(x => x.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Producto no encontrado");

        _unitOfWorkMock.Verify(x => x.Products.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateCode_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var existingProduct = new Product
        {
            Id = productId,
            Code = "7750001001001",
            Name = "Producto Original",
            SalePrice = 10.00m,
            CategoryId = categoryId
        };

        var duplicateProduct = new Product
        {
            Id = otherProductId,
            Code = "7750001001999", // Código que se quiere usar
            Name = "Otro Producto",
            SalePrice = 10.00m
        };

        var command = new UpdateProductCommand
        {
            Product = new UpdateProductDto
            {
                Id = productId,
                Code = "7750001001999", // Código duplicado
                Name = "Producto Actualizado",
                CategoryId = categoryId
            }
        };

        _unitOfWorkMock
            .Setup(x => x.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { duplicateProduct });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("código ya existe");

        _unitOfWorkMock.Verify(x => x.Products.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

