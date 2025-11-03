using FluentAssertions;
using Minimarket.Application.Features.Sales.Commands;
using Minimarket.Application.Features.Sales.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;
using Moq;
using Xunit;

namespace Minimarket.UnitTests.Application.Validators;

public class CreateSaleCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateSaleCommandValidator _validator;

    public CreateSaleCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validator = new CreateSaleCommandValidator(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Stock = 100,
            IsActive = true,
            SalePrice = 3.50m
        };

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

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
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptySaleDetails_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 25.00m,
                SaleDetails = new List<CreateSaleDetailDto>()
            },
            UserId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sale.SaleDetails");
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 25.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 5, UnitPrice = 3.50m }
                }
            },
            UserId = Guid.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public async Task Validate_WithEmptySaleDetails_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 25.00m,
                SaleDetails = new List<CreateSaleDetailDto>()
            },
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sale.SaleDetails");
    }

    [Fact]
    public async Task Validate_WithDiscountExceedingSubtotal_ShouldBeInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Stock = 100,
            IsActive = true,
            SalePrice = 10.00m
        };

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 100.00m,
                Discount = 1000.00m, // Descuento mayor al subtotal
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 5, UnitPrice = 10.00m }
                }
            },
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("descuento"));
    }

    [Fact]
    public async Task Validate_WithAmountPaidLessThanTotal_ShouldBeInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Stock = 100,
            IsActive = true,
            SalePrice = 10.00m
        };

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 5.00m, // Menor que el total (que será ~59.00)
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 5, UnitPrice = 10.00m }
                }
            },
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("monto pagado"));
    }

    [Fact]
    public async Task Validate_WithFacturaWithoutCustomer_ShouldBeInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Stock = 100,
            IsActive = true,
            SalePrice = 10.00m
        };

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Factura, // Factura requiere cliente
                CustomerId = null, // Sin cliente
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 100.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 5, UnitPrice = 10.00m }
                }
            },
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("factura") || e.ErrorMessage.Contains("cliente"));
    }

    [Fact]
    public async Task Validate_WithNonExistentProducts_ShouldBeInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>()); // Productos no existen

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
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("producto") || e.ErrorMessage.Contains("existe"));
    }

    [Fact]
    public async Task Validate_WithInsufficientStock_ShouldBeInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Stock = 2, // Stock insuficiente
            IsActive = true,
            SalePrice = 10.00m
        };

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 100.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = productId, Quantity = 10, UnitPrice = 10.00m } // Pide 10 pero solo hay 2
                }
            },
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Stock") || e.ErrorMessage.Contains("insuficiente"));
    }

    [Fact]
    public async Task Validate_WithInactiveProducts_ShouldBeInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Stock = 100,
            IsActive = false, // Inactivo
            SalePrice = 10.00m
        };

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

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
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("inactivo"));
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Stock = 100,
            IsActive = true,
            SalePrice = 10.00m
        };

        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            DocumentType = "DNI",
            DocumentNumber = "12345678"
        };

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        _unitOfWorkMock
            .Setup(x => x.Customers.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

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
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidSaleDetailQuantity_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 25.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 0, UnitPrice = 3.50m }
                }
            },
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Quantity"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Validate_WithInvalidSaleDetailUnitPrice_ShouldBeInvalid(decimal unitPrice)
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 25.00m,
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 5, UnitPrice = unitPrice }
                }
            },
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("UnitPrice"));
    }
}

