using FluentAssertions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Queries;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;
using Moq;
using Xunit;

namespace Minimarket.UnitTests.Application.Queries;

public class GetSaleByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GetSaleByIdQueryHandler _handler;

    public GetSaleByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new GetSaleByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnSale()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var sale = new Sale
        {
            Id = saleId,
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            SaleDate = DateTime.UtcNow,
            Status = SaleStatus.Pagado,
            Subtotal = 100.00m,
            Tax = 18.00m,
            Total = 118.00m,
            PaymentMethod = PaymentMethod.Efectivo,
            AmountPaid = 150.00m,
            Change = 32.00m,
            UserId = userId
        };

        var saleDetail = new SaleDetail
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            ProductId = productId,
            Quantity = 5,
            UnitPrice = 10.00m,
            Subtotal = 50.00m
        };

        var product = new Product
        {
            Id = productId,
            Code = "7750001001001",
            Name = "Producto Test",
            SalePrice = 10.00m
        };

        var saleDetails = new List<SaleDetail> { saleDetail };
        var products = new List<Product> { product };

        var query = new GetSaleByIdQuery { Id = saleId };

        _unitOfWorkMock
            .Setup(x => x.Sales.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        _unitOfWorkMock
            .Setup(x => x.SaleDetails.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SaleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(saleDetails);

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(saleId);
        result.Data.DocumentNumber.Should().Be("B001-00000001");
        result.Data.Total.Should().Be(118.00m);
        result.Data.SaleDetails.Should().HaveCount(1);
        result.Data.SaleDetails[0].ProductName.Should().Be("Producto Test");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailure()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var query = new GetSaleByIdQuery { Id = saleId };

        _unitOfWorkMock
            .Setup(x => x.Sales.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Venta no encontrada");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithCustomer_ShouldIncludeCustomerName()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            Name = "Juan Pérez",
            DocumentType = "DNI",
            DocumentNumber = "12345678"
        };

        var sale = new Sale
        {
            Id = saleId,
            DocumentNumber = "F001-00000001",
            DocumentType = DocumentType.Factura,
            CustomerId = customerId,
            Status = SaleStatus.Pagado,
            Total = 118.00m,
            UserId = userId
        };

        var query = new GetSaleByIdQuery { Id = saleId };

        _unitOfWorkMock
            .Setup(x => x.Sales.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        _unitOfWorkMock
            .Setup(x => x.Customers.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _unitOfWorkMock
            .Setup(x => x.SaleDetails.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SaleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SaleDetail>());

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.CustomerName.Should().Be("Juan Pérez");
    }
}

