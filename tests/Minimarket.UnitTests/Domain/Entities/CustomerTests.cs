using FluentAssertions;
using Minimarket.Domain.Entities;
using Xunit;

namespace Minimarket.UnitTests.Domain.Entities;

public class CustomerTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCustomer()
    {
        // Arrange & Act
        var customer = new Customer
        {
            DocumentType = "DNI",
            DocumentNumber = "12345678",
            Name = "Juan Pérez",
            Email = "juan.perez@example.com",
            Phone = "987654321",
            Address = "Av. Principal 123",
            IsActive = true
        };

        // Assert
        customer.Should().NotBeNull();
        customer.DocumentType.Should().Be("DNI");
        customer.DocumentNumber.Should().Be("12345678");
        customer.Name.Should().Be("Juan Pérez");
        customer.Email.Should().Be("juan.perez@example.com");
        customer.Phone.Should().Be("987654321");
        customer.Address.Should().Be("Av. Principal 123");
        customer.IsActive.Should().BeTrue();
        customer.Id.Should().NotBeEmpty();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithRUC_ShouldAcceptRUC()
    {
        // Arrange & Act
        var customer = new Customer
        {
            DocumentType = "RUC",
            DocumentNumber = "20123456789",
            Name = "Empresa SAC",
            IsActive = true
        };

        // Assert
        customer.DocumentType.Should().Be("RUC");
        customer.DocumentNumber.Should().Be("20123456789");
    }

    [Fact]
    public void Create_WithOptionalFields_ShouldAllowNullOptionalFields()
    {
        // Arrange & Act
        var customer = new Customer
        {
            DocumentType = "DNI",
            DocumentNumber = "12345678",
            Name = "Juan Pérez",
            Email = null,
            Phone = null,
            Address = null,
            IsActive = true
        };

        // Assert
        customer.Email.Should().BeNull();
        customer.Phone.Should().BeNull();
        customer.Address.Should().BeNull();
    }

    [Fact]
    public void Deactivate_ActiveCustomer_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var customer = new Customer
        {
            DocumentType = "DNI",
            DocumentNumber = "12345678",
            Name = "Juan Pérez",
            IsActive = true
        };

        // Act
        customer.IsActive = false;

        // Assert
        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldUpdateEmail()
    {
        // Arrange
        var customer = new Customer
        {
            DocumentType = "DNI",
            DocumentNumber = "12345678",
            Name = "Juan Pérez",
            Email = "old@example.com"
        };

        // Act
        customer.Email = "new@example.com";

        // Assert
        customer.Email.Should().Be("new@example.com");
    }
}

