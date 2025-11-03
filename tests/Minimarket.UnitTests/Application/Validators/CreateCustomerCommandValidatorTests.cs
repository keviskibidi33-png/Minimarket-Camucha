using FluentAssertions;
using Minimarket.Application.Features.Customers.Commands;
using Minimarket.Application.Features.Customers.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;
using Moq;
using Xunit;

namespace Minimarket.UnitTests.Application.Validators;

public class CreateCustomerCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateCustomerCommandValidator _validator;

    public CreateCustomerCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validator = new CreateCustomerCommandValidator(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Customer = new CreateCustomerDto
            {
                DocumentType = "DNI",
                DocumentNumber = "12345678",
                Name = "Juan Pérez",
                Phone = "987654321",
                Email = "juan@example.com"
            }
        };

        _unitOfWorkMock
            .Setup(x => x.Customers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("1234567")]  // 7 dígitos
    [InlineData("123456789")]  // 9 dígitos
    [InlineData("1234567a")]  // Contiene letra
    public async Task Validate_WithInvalidDNI_ShouldBeInvalid(string dni)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Customer = new CreateCustomerDto
            {
                DocumentType = "DNI",
                DocumentNumber = dni,
                Name = "Juan Pérez"
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("DocumentNumber"));
    }

    [Theory]
    [InlineData("2012345678")]  // 10 dígitos
    [InlineData("201234567890")]  // 12 dígitos
    [InlineData("2012345678a")]  // Contiene letra
    public async Task Validate_WithInvalidRUC_ShouldBeInvalid(string ruc)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Customer = new CreateCustomerDto
            {
                DocumentType = "RUC",
                DocumentNumber = ruc,
                Name = "Empresa SAC"
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("DocumentNumber"));
    }

    [Theory]
    [InlineData("81234567")]  // No empieza con 9
    [InlineData("123456789")]  // 9 dígitos pero no empieza con 9
    [InlineData("9876543")]  // 7 dígitos
    [InlineData("9876543210")]  // 10 dígitos
    public async Task Validate_WithInvalidPeruvianPhone_ShouldBeInvalid(string phone)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Customer = new CreateCustomerDto
            {
                DocumentType = "DNI",
                DocumentNumber = "12345678",
                Name = "Juan Pérez",
                Phone = phone
            }
        };

        _unitOfWorkMock
            .Setup(x => x.Customers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Phone"));
    }

    [Theory]
    [InlineData("987654321")]
    [InlineData("987654321 ")]
    [InlineData("987-654-321")]
    [InlineData("(987) 654-321")]
    public async Task Validate_WithValidPeruvianPhone_ShouldBeValid(string phone)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Customer = new CreateCustomerDto
            {
                DocumentType = "DNI",
                DocumentNumber = "12345678",
                Name = "Juan Pérez",
                Phone = phone
            }
        };

        _unitOfWorkMock
            .Setup(x => x.Customers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithDuplicateDocument_ShouldBeInvalid()
    {
        // Arrange
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            DocumentType = "DNI",
            DocumentNumber = "12345678",
            Name = "Cliente Existente"
        };

        var command = new CreateCustomerCommand
        {
            Customer = new CreateCustomerDto
            {
                DocumentType = "DNI",
                DocumentNumber = "12345678", // Duplicado
                Name = "Juan Pérez"
            }
        };

        _unitOfWorkMock
            .Setup(x => x.Customers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer> { existingCustomer });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("documento"));
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Customer = new CreateCustomerDto
            {
                DocumentType = "DNI",
                DocumentNumber = "12345678",
                Name = string.Empty
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Name"));
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Customer = new CreateCustomerDto
            {
                DocumentType = "DNI",
                DocumentNumber = "12345678",
                Name = "Juan Pérez",
                Email = "invalid-email"
            }
        };

        _unitOfWorkMock
            .Setup(x => x.Customers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Email"));
    }

    [Fact]
    public async Task Validate_WithInvalidDocumentType_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Customer = new CreateCustomerDto
            {
                DocumentType = "PASAPORTE", // Inválido
                DocumentNumber = "12345678",
                Name = "Juan Pérez"
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("DocumentType"));
    }
}

