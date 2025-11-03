using FluentAssertions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Commands;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;
using Moq;
using Xunit;

namespace Minimarket.UnitTests.Application.Commands;

public class CancelSaleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CancelSaleCommandHandler _handler;

    public CancelSaleCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CancelSaleCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidSale_ShouldCancelSaleAndRestoreStock()
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
            Status = SaleStatus.Pagado,
            Total = 118.00m,
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
            Stock = 95, // Stock después de la venta
            SalePrice = 10.00m
        };

        var saleDetails = new List<SaleDetail> { saleDetail };

        var command = new CancelSaleCommand
        {
            SaleId = saleId,
            Reason = "Cancelado por cliente",
            UserId = userId
        };

        _unitOfWorkMock
            .Setup(x => x.Sales.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        _unitOfWorkMock
            .Setup(x => x.SaleDetails.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SaleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(saleDetails);

        _unitOfWorkMock
            .Setup(x => x.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.Products.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.Sales.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.Sales.UpdateAsync(
            It.Is<Sale>(s => s.Status == SaleStatus.Anulado && s.CancellationReason == "Cancelado por cliente"),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.Products.UpdateAsync(
            It.Is<Product>(p => p.Stock == 100), // Stock restaurado
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentSale_ShouldReturnFailure()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var command = new CancelSaleCommand
        {
            SaleId = saleId,
            Reason = "Cancelado por cliente",
            UserId = userId
        };

        _unitOfWorkMock
            .Setup(x => x.Sales.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Venta no encontrada");

        _unitOfWorkMock.Verify(x => x.Sales.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyCancelledSale_ShouldReturnFailure()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var sale = new Sale
        {
            Id = saleId,
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            Status = SaleStatus.Anulado, // Ya anulada
            Total = 118.00m,
            UserId = userId
        };

        var command = new CancelSaleCommand
        {
            SaleId = saleId,
            Reason = "Intento de cancelación duplicada",
            UserId = userId
        };

        _unitOfWorkMock
            .Setup(x => x.Sales.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ya está anulada");

        _unitOfWorkMock.Verify(x => x.Sales.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

