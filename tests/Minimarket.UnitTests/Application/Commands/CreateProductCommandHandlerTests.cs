using FluentAssertions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.Commands;
using Minimarket.Application.Features.Products.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;
using Moq;
using Xunit;

namespace Minimarket.UnitTests.Application.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Bebidas",
            IsActive = true
        };

        var command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Code = "7750001001999",
                Name = "Producto de Prueba",
                Description = "Descripción del producto",
                PurchasePrice = 5.00m,
                SalePrice = 8.00m,
                Stock = 20,
                MinimumStock = 5,
                CategoryId = categoryId
            }
        };

        var existingProducts = new List<Product>(); // No hay productos con ese código

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProducts);

        _unitOfWorkMock
            .Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _unitOfWorkMock
            .Setup(x => x.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be("7750001001999");
        result.Data.Name.Should().Be("Producto de Prueba");
        result.Data.SalePrice.Should().Be(8.00m);

        _unitOfWorkMock.Verify(x => x.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateCode_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            Code = "7750001001999",
            Name = "Producto Existente",
            SalePrice = 10.00m
        };

        var command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Code = "7750001001999", // Código duplicado
                Name = "Producto Nuevo",
                PurchasePrice = 5.00m,
                SalePrice = 8.00m,
                Stock = 20,
                CategoryId = categoryId
            }
        };

        var existingProducts = new List<Product> { existingProduct };

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProducts);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("código ya existe");

        _unitOfWorkMock.Verify(x => x.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        var command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Code = "7750001001999",
                Name = "Producto de Prueba",
                PurchasePrice = 5.00m,
                SalePrice = 8.00m,
                Stock = 20,
                CategoryId = categoryId
            }
        };

        var existingProducts = new List<Product>();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProducts);

        _unitOfWorkMock
            .Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("categoría");

        _unitOfWorkMock.Verify(x => x.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

