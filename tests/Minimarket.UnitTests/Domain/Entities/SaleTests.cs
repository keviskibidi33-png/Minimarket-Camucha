using FluentAssertions;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Xunit;

namespace Minimarket.UnitTests.Domain.Entities;

public class SaleTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSale()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Act
        var sale = new Sale
        {
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            SaleDate = DateTime.UtcNow,
            CustomerId = customerId,
            Subtotal = 100.00m,
            Tax = 18.00m,
            Discount = 0m,
            Total = 118.00m,
            PaymentMethod = PaymentMethod.Efectivo,
            AmountPaid = 150.00m,
            Change = 32.00m,
            Status = SaleStatus.Pagado,
            UserId = userId
        };

        // Assert
        sale.Should().NotBeNull();
        sale.DocumentNumber.Should().Be("B001-00000001");
        sale.DocumentType.Should().Be(DocumentType.Boleta);
        sale.Subtotal.Should().Be(100.00m);
        sale.Tax.Should().Be(18.00m);
        sale.Total.Should().Be(118.00m);
        sale.Change.Should().Be(32.00m);
        sale.Status.Should().Be(SaleStatus.Pagado);
        sale.Id.Should().NotBeEmpty();
        sale.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(100.00, 18.00, 118.00)] // Subtotal, IGV esperado, Total esperado
    [InlineData(50.50, 9.09, 59.59)]
    [InlineData(0.01, 0.00, 0.01)] // Edge case: valor muy pequeño
    [InlineData(1000.00, 180.00, 1180.00)]
    public void CalculateTotal_WithDifferentAmounts_ShouldCalculateCorrectly(
        decimal subtotal, decimal expectedTax, decimal expectedTotal)
    {
        // Arrange
        var discount = 0m;
        const decimal IGV_RATE = 0.18m;

        // Act
        var subtotalAfterDiscount = Math.Round(subtotal - discount, 2, MidpointRounding.AwayFromZero);
        var tax = Math.Round(subtotalAfterDiscount * IGV_RATE, 2, MidpointRounding.AwayFromZero);
        var total = Math.Round(subtotalAfterDiscount + tax, 2, MidpointRounding.AwayFromZero);

        // Assert
        tax.Should().Be(expectedTax);
        total.Should().Be(expectedTotal);
    }

    [Fact]
    public void CalculateTotal_WithDiscount_ShouldApplyDiscountCorrectly()
    {
        // Arrange
        var subtotal = 100.00m;
        var discount = 10.00m;
        const decimal IGV_RATE = 0.18m;

        // Act
        var subtotalAfterDiscount = Math.Round(subtotal - discount, 2, MidpointRounding.AwayFromZero);
        var tax = Math.Round(subtotalAfterDiscount * IGV_RATE, 2, MidpointRounding.AwayFromZero);
        var total = Math.Round(subtotalAfterDiscount + tax, 2, MidpointRounding.AwayFromZero);

        // Assert
        subtotalAfterDiscount.Should().Be(90.00m);
        tax.Should().Be(16.20m);
        total.Should().Be(106.20m);
    }

    [Fact]
    public void CalculateChange_WithSufficientPayment_ShouldCalculateCorrectly()
    {
        // Arrange
        var total = 118.00m;
        var amountPaid = 150.00m;

        // Act
        var change = Math.Round(amountPaid - total, 2, MidpointRounding.AwayFromZero);

        // Assert
        change.Should().Be(32.00m);
    }

    [Fact]
    public void CalculateChange_WithExactPayment_ShouldReturnZero()
    {
        // Arrange
        var total = 118.00m;
        var amountPaid = 118.00m;

        // Act
        var change = Math.Round(amountPaid - total, 2, MidpointRounding.AwayFromZero);

        // Assert
        change.Should().Be(0.00m);
    }

    [Fact]
    public void Cancel_ValidSale_ShouldChangeStatusToAnulado()
    {
        // Arrange
        var sale = new Sale
        {
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            Status = SaleStatus.Pagado,
            Total = 118.00m
        };

        // Act
        sale.Status = SaleStatus.Anulado;
        sale.CancellationReason = "Cancelado por cliente";
        sale.UpdatedAt = DateTime.UtcNow;

        // Assert
        sale.Status.Should().Be(SaleStatus.Anulado);
        sale.CancellationReason.Should().Be("Cancelado por cliente");
        sale.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CreateFactura_RequiresCustomer_ShouldHaveCustomerId()
    {
        // Arrange & Act
        var sale = new Sale
        {
            DocumentNumber = "F001-00000001",
            DocumentType = DocumentType.Factura,
            CustomerId = Guid.NewGuid(),
            Status = SaleStatus.Pagado,
            Total = 118.00m
        };

        // Assert
        sale.DocumentType.Should().Be(DocumentType.Factura);
        sale.CustomerId.Should().NotBeNull();
    }

    [Fact]
    public void CreateBoleta_CanBeWithoutCustomer_ShouldAllowNullCustomer()
    {
        // Arrange & Act
        var sale = new Sale
        {
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            CustomerId = null,
            Status = SaleStatus.Pagado,
            Total = 118.00m
        };

        // Assert
        sale.DocumentType.Should().Be(DocumentType.Boleta);
        sale.CustomerId.Should().BeNull();
    }

    [Theory]
    [InlineData(PaymentMethod.Efectivo)]
    [InlineData(PaymentMethod.Tarjeta)]
    [InlineData(PaymentMethod.YapePlin)]
    [InlineData(PaymentMethod.Transferencia)]
    public void CreateSale_WithDifferentPaymentMethods_ShouldAcceptAllMethods(PaymentMethod paymentMethod)
    {
        // Arrange & Act
        var sale = new Sale
        {
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            PaymentMethod = paymentMethod,
            Status = SaleStatus.Pagado,
            Total = 118.00m
        };

        // Assert
        sale.PaymentMethod.Should().Be(paymentMethod);
    }
}

