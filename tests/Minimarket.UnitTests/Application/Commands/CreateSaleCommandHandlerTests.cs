using FluentAssertions;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Commands;
using Minimarket.Application.Features.Sales.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;
using Moq;
using Xunit;

namespace Minimarket.UnitTests.Application.Commands;

public class CreateSaleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateSaleCommandHandler>> _loggerMock;
    private readonly CreateSaleCommandHandler _handler;

    public CreateSaleCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateSaleCommandHandler>>();
        
        _handler = new CreateSaleCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateSaleAndUpdateStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        var product = new Product
        {
            Id = productId,
            Code = "7750001001001",
            Name = "Coca Cola 500ml",
            Stock = 100,
            SalePrice = 3.50m,
            IsActive = true,
            CategoryId = categoryId
        };

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 25.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 5, UnitPrice = 3.50m }
                }
            },
            UserId = userId
        };

        var products = new List<Product> { product };
        var sales = new List<Sale>();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Sales.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        _unitOfWorkMock
            .Setup(x => x.Sales.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Total.Should().BeGreaterThan(0);
        result.Data.DocumentNumber.Should().MatchRegex(@"^B\d{3}-\d{8}$");

        _unitOfWorkMock.Verify(x => x.Products.UpdateAsync(It.Is<Product>(p => p.Stock == 95), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Sales.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ShouldThrowInsufficientStockException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Code = "7750001001001",
            Name = "Producto con poco stock",
            Stock = 2, // Solo 2 en stock
            SalePrice = 5.00m,
            IsActive = true,
            CategoryId = categoryId
        };

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 100.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 10, UnitPrice = 5.00m } // Intenta vender 10
                }
            },
            UserId = userId
        };

        var products = new List<Product> { product };

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InsufficientStockException>()
            .WithMessage("*Stock insuficiente*");

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Sales.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldThrowNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 10.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 1, UnitPrice = 10.00m }
                }
            },
            UserId = userId
        };

        var products = new List<Product>(); // Lista vacía

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Product*");

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInactiveProduct_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Code = "7750001001001",
            Name = "Producto Inactivo",
            Stock = 100,
            SalePrice = 5.00m,
            IsActive = false, // Inactivo
            CategoryId = categoryId
        };

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 10.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 1, UnitPrice = 5.00m }
                }
            },
            UserId = userId
        };

        var products = new List<Product> { product };

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("*inactivo*");

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInsufficientPayment_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Code = "7750001001001",
            Name = "Producto Caro",
            Stock = 100,
            SalePrice = 100.00m,
            IsActive = true,
            CategoryId = categoryId
        };

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 50.00m, // Insuficiente
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 1, UnitPrice = 100.00m }
                }
            },
            UserId = userId
        };

        var products = new List<Product> { product };
        var sales = new List<Sale>();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Sales.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        _unitOfWorkMock
            .Setup(x => x.Sales.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("*monto pagado*");

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCalculateSubtotalCorrectly()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Stock = 100,
            SalePrice = 10.00m,
            IsActive = true,
            CategoryId = categoryId
        };

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 100.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 5, UnitPrice = 10.00m },
                    new() { ProductId = productId, Quantity = 3, UnitPrice = 5.00m }
                }
            },
            UserId = userId
        };

        var products = new List<Product> { product };
        var sales = new List<Sale>();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Sales.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        _unitOfWorkMock
            .Setup(x => x.Sales.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Subtotal.Should().Be(65.00m); // (5 * 10) + (3 * 5) = 50 + 15 = 65
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCalculateIGVCorrectly()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Stock = 100,
            SalePrice = 100.00m,
            IsActive = true,
            CategoryId = categoryId
        };

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 200.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 1, UnitPrice = 100.00m }
                }
            },
            UserId = userId
        };

        var products = new List<Product> { product };
        var sales = new List<Sale>();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Sales.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        _unitOfWorkMock
            .Setup(x => x.Sales.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Tax.Should().Be(18.00m); // 100 * 0.18 = 18.00
        result.Data.Total.Should().Be(118.00m); // 100 + 18 = 118
    }

    [Fact]
    public async Task Handle_WithDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Stock = 100,
            SalePrice = 100.00m,
            IsActive = true,
            CategoryId = categoryId
        };

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 120.00m,
                Discount = 10.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 1, UnitPrice = 100.00m }
                }
            },
            UserId = userId
        };

        var products = new List<Product> { product };
        var sales = new List<Sale>();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Sales.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        _unitOfWorkMock
            .Setup(x => x.Sales.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Subtotal.Should().Be(100.00m);
        result.Data.Discount.Should().Be(10.00m);
        // Subtotal después de descuento: 90.00
        // IGV: 90 * 0.18 = 16.20
        // Total: 90 + 16.20 = 106.20
        result.Data.Tax.Should().Be(16.20m);
        result.Data.Total.Should().Be(106.20m);
        result.Data.Change.Should().Be(13.80m); // 120 - 106.20
    }

    [Fact]
    public async Task Handle_WithDiscountExceedingSubtotal_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Stock = 100,
            SalePrice = 100.00m,
            IsActive = true,
            CategoryId = categoryId
        };

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 200.00m,
                Discount = 150.00m, // Descuento mayor al subtotal
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 1, UnitPrice = 100.00m }
                }
            },
            UserId = userId
        };

        var products = new List<Product> { product };
        var sales = new List<Sale>();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Sales.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        _unitOfWorkMock
            .Setup(x => x.Sales.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("*descuento*");

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueDocumentNumber()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Stock = 100,
            SalePrice = 10.00m,
            IsActive = true,
            CategoryId = categoryId
        };

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 100.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 5, UnitPrice = 10.00m }
                }
            },
            UserId = userId
        };

        var products = new List<Product> { product };
        var sales = new List<Sale>();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Sales.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        _unitOfWorkMock
            .Setup(x => x.Sales.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.DocumentNumber.Should().MatchRegex(@"^B\d{3}-\d{8}$");
    }

    [Fact]
    public async Task Handle_ShouldRoundAmountsToTwoDecimals()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Stock = 100,
            SalePrice = 10.333m, // Precio con decimales
            IsActive = true,
            CategoryId = categoryId
        };

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 100.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 3, UnitPrice = 10.333m }
                }
            },
            UserId = userId
        };

        var products = new List<Product> { product };
        var sales = new List<Sale>();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Sales.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sales);

        _unitOfWorkMock
            .Setup(x => x.Sales.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Verificar que los montos están redondeados a 2 decimales
        result.Data!.Subtotal.Should().Be(Math.Round(3 * 10.333m, 2));
        result.Data.Tax.Should().Be(Math.Round(result.Data.Subtotal * 0.18m, 2));
        result.Data.Total.Should().Be(Math.Round(result.Data.Subtotal + result.Data.Tax, 2));
    }
}

