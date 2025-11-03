using FluentAssertions;
using Minimarket.Application.Features.Products.Commands;
using Minimarket.Application.Features.Products.DTOs;
using Xunit;

namespace Minimarket.UnitTests.Application.Validators;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        _validator = new CreateProductCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Code = "7750001001001",
                Name = "Producto Test",
                Description = "Descripción del producto",
                PurchasePrice = 5.00m,
                SalePrice = 10.00m,
                Stock = 100,
                MinimumStock = 10,
                CategoryId = Guid.NewGuid()
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCode_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Code = string.Empty,
                Name = "Producto Test",
                PurchasePrice = 5.00m,
                SalePrice = 10.00m,
                Stock = 100,
                CategoryId = Guid.NewGuid()
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Product.Code");
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Code = "7750001001001",
                Name = string.Empty,
                PurchasePrice = 5.00m,
                SalePrice = 10.00m,
                Stock = 100,
                CategoryId = Guid.NewGuid()
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Product.Name");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Validate_WithInvalidPurchasePrice_ShouldBeInvalid(decimal purchasePrice)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Code = "7750001001001",
                Name = "Producto Test",
                PurchasePrice = purchasePrice,
                SalePrice = 10.00m,
                Stock = 100,
                CategoryId = Guid.NewGuid()
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Product.PurchasePrice");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Validate_WithInvalidSalePrice_ShouldBeInvalid(decimal salePrice)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Code = "7750001001001",
                Name = "Producto Test",
                PurchasePrice = 5.00m,
                SalePrice = salePrice,
                Stock = 100,
                CategoryId = Guid.NewGuid()
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Product.SalePrice");
    }

    [Fact]
    public void Validate_WithNegativeStock_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Code = "7750001001001",
                Name = "Producto Test",
                PurchasePrice = 5.00m,
                SalePrice = 10.00m,
                Stock = -1,
                CategoryId = Guid.NewGuid()
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Product.Stock");
    }

    [Fact]
    public void Validate_WithEmptyCategoryId_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Code = "7750001001001",
                Name = "Producto Test",
                PurchasePrice = 5.00m,
                SalePrice = 10.00m,
                Stock = 100,
                CategoryId = Guid.Empty
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Product.CategoryId");
    }
}

