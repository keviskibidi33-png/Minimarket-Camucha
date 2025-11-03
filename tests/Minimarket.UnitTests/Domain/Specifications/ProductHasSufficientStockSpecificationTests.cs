using FluentAssertions;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Specifications;
using Xunit;

namespace Minimarket.UnitTests.Domain.Specifications;

public class ProductHasSufficientStockSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithSufficientStock_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            Stock = 100,
            IsActive = true,
            SalePrice = 10.00m
        };
        var specification = new ProductHasSufficientStockSpecification(50);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithExactStock_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            Stock = 50,
            IsActive = true,
            SalePrice = 10.00m
        };
        var specification = new ProductHasSufficientStockSpecification(50);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            Stock = 30,
            IsActive = true,
            SalePrice = 10.00m
        };
        var specification = new ProductHasSufficientStockSpecification(50);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithInactiveProduct_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            Stock = 100,
            IsActive = false,
            SalePrice = 10.00m
        };
        var specification = new ProductHasSufficientStockSpecification(50);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithNullProduct_ShouldReturnFalse()
    {
        // Arrange
        Product? product = null;
        var specification = new ProductHasSufficientStockSpecification(50);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithZeroQuantity_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => new ProductHasSufficientStockSpecification(0);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*cantidad requerida debe ser mayor a 0*");
    }

    [Fact]
    public void Constructor_WithNegativeQuantity_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => new ProductHasSufficientStockSpecification(-1);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*cantidad requerida debe ser mayor a 0*");
    }

    [Fact]
    public void ToExpression_ShouldReturnCorrectExpression()
    {
        // Arrange
        var specification = new ProductHasSufficientStockSpecification(50);
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            Stock = 100,
            IsActive = true,
            SalePrice = 10.00m
        };

        // Act
        var expression = specification.ToExpression();
        var compiled = expression.Compile();
        var result = compiled(product);

        // Assert
        result.Should().BeTrue();
    }
}

