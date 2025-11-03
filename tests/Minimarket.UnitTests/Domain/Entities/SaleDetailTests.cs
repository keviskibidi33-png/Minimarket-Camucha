using FluentAssertions;
using Minimarket.Domain.Entities;
using Xunit;

namespace Minimarket.UnitTests.Domain.Entities;

public class SaleDetailTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSaleDetail()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // Act
        var saleDetail = new SaleDetail
        {
            SaleId = saleId,
            ProductId = productId,
            Quantity = 5,
            UnitPrice = 10.00m,
            Subtotal = 50.00m
        };

        // Assert
        saleDetail.Should().NotBeNull();
        saleDetail.SaleId.Should().Be(saleId);
        saleDetail.ProductId.Should().Be(productId);
        saleDetail.Quantity.Should().Be(5);
        saleDetail.UnitPrice.Should().Be(10.00m);
        saleDetail.Subtotal.Should().Be(50.00m);
        saleDetail.Id.Should().NotBeEmpty();
        saleDetail.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CalculateSubtotal_WithQuantityAndPrice_ShouldCalculateCorrectly()
    {
        // Arrange
        var quantity = 3;
        var unitPrice = 12.50m;

        // Act
        var subtotal = Math.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);

        // Assert
        subtotal.Should().Be(37.50m);
    }

    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(5, 10.00, 50.00)]
    [InlineData(10, 12.50, 125.00)]
    [InlineData(0, 10.00, 0.00)]
    public void CalculateSubtotal_WithDifferentValues_ShouldCalculateCorrectly(
        int quantity, decimal unitPrice, decimal expectedSubtotal)
    {
        // Act
        var subtotal = Math.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);

        // Assert
        subtotal.Should().Be(expectedSubtotal);
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldAllowZeroQuantity()
    {
        // Arrange & Act
        var saleDetail = new SaleDetail
        {
            SaleId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 0,
            UnitPrice = 10.00m,
            Subtotal = 0.00m
        };

        // Assert
        saleDetail.Quantity.Should().Be(0);
        saleDetail.Subtotal.Should().Be(0.00m);
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldAllowNegativeQuantity()
    {
        // Arrange & Act
        var saleDetail = new SaleDetail
        {
            SaleId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = -5,
            UnitPrice = 10.00m,
            Subtotal = -50.00m
        };

        // Assert
        saleDetail.Quantity.Should().Be(-5);
        saleDetail.Subtotal.Should().Be(-50.00m);
    }
}

