using FluentAssertions;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Specifications;
using Xunit;

namespace Minimarket.UnitTests.Domain.Specifications;

public class ProductIsActiveSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithActiveProduct_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            IsActive = true,
            SalePrice = 10.00m
        };
        var specification = new ProductIsActiveSpecification();

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithInactiveProduct_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            IsActive = false,
            SalePrice = 10.00m
        };
        var specification = new ProductIsActiveSpecification();

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
        var specification = new ProductIsActiveSpecification();

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ToExpression_ShouldReturnCorrectExpression()
    {
        // Arrange
        var specification = new ProductIsActiveSpecification();
        var activeProduct = new Product
        {
            Code = "7750001001001",
            Name = "Producto Activo",
            IsActive = true,
            SalePrice = 10.00m
        };
        var inactiveProduct = new Product
        {
            Code = "7750001001002",
            Name = "Producto Inactivo",
            IsActive = false,
            SalePrice = 10.00m
        };

        // Act
        var expression = specification.ToExpression();
        var compiled = expression.Compile();
        var activeResult = compiled(activeProduct);
        var inactiveResult = compiled(inactiveProduct);

        // Assert
        activeResult.Should().BeTrue();
        inactiveResult.Should().BeFalse();
    }
}

