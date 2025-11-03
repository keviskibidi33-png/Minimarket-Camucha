using FluentAssertions;
using Minimarket.Domain.Entities;
using Xunit;

namespace Minimarket.UnitTests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        
        // Act
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Coca Cola 500ml",
            Description = "Bebida gaseosa",
            PurchasePrice = 2.00m,
            SalePrice = 3.50m,
            Stock = 100,
            MinimumStock = 10,
            CategoryId = categoryId,
            IsActive = true
        };

        // Assert
        product.Should().NotBeNull();
        product.Code.Should().Be("7750001001001");
        product.Name.Should().Be("Coca Cola 500ml");
        product.PurchasePrice.Should().Be(2.00m);
        product.SalePrice.Should().Be(3.50m);
        product.Stock.Should().Be(100);
        product.MinimumStock.Should().Be(10);
        product.IsActive.Should().BeTrue();
        product.Id.Should().NotBeEmpty();
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateStock_WithValidQuantity_ShouldDecreaseStock()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            Stock = 100,
            SalePrice = 10.00m
        };

        // Act
        product.Stock -= 25;

        // Assert
        product.Stock.Should().Be(75);
    }

    [Fact]
    public void UpdateStock_ToNegative_ShouldAllowNegativeStock()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            Stock = 10,
            SalePrice = 10.00m
        };

        // Act
        product.Stock -= 15;

        // Assert
        product.Stock.Should().Be(-5);
    }

    [Theory]
    [InlineData(100, 10, true)]  // Stock por encima del mínimo
    [InlineData(10, 10, false)]  // Stock igual al mínimo
    [InlineData(5, 10, false)]    // Stock por debajo del mínimo
    [InlineData(0, 10, false)]   // Stock cero
    public void IsBelowMinimumStock_WithDifferentStockLevels_ShouldReturnCorrectValue(
        int stock, int minimumStock, bool expectedAboveMinimum)
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            Stock = stock,
            MinimumStock = minimumStock,
            SalePrice = 10.00m
        };

        // Act
        var isAboveMinimum = product.Stock > product.MinimumStock;

        // Assert
        isAboveMinimum.Should().Be(expectedAboveMinimum);
    }

    [Fact]
    public void Deactivate_ActiveProduct_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            IsActive = true,
            SalePrice = 10.00m
        };

        // Act
        product.IsActive = false;

        // Assert
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdateSalePrice()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            SalePrice = 10.00m
        };

        // Act
        product.SalePrice = 12.50m;

        // Assert
        product.SalePrice.Should().Be(12.50m);
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ShouldAllowNegativePrice()
    {
        // Arrange
        var product = new Product
        {
            Code = "7750001001001",
            Name = "Producto Test",
            SalePrice = 10.00m
        };

        // Act
        product.SalePrice = -5.00m;

        // Assert
        product.SalePrice.Should().Be(-5.00m);
    }
}

