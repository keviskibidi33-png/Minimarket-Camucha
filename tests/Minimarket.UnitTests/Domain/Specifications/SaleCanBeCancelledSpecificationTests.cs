using FluentAssertions;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Specifications;
using Xunit;

namespace Minimarket.UnitTests.Domain.Specifications;

public class SaleCanBeCancelledSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithPagadoStatus_ShouldReturnTrue()
    {
        // Arrange
        var sale = new Sale
        {
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            Status = SaleStatus.Pagado,
            Total = 118.00m
        };
        var specification = new SaleCanBeCancelledSpecification();

        // Act
        var result = specification.IsSatisfiedBy(sale);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithPendienteStatus_ShouldReturnTrue()
    {
        // Arrange
        var sale = new Sale
        {
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            Status = SaleStatus.Pendiente,
            Total = 118.00m
        };
        var specification = new SaleCanBeCancelledSpecification();

        // Act
        var result = specification.IsSatisfiedBy(sale);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithAnuladoStatus_ShouldReturnFalse()
    {
        // Arrange
        var sale = new Sale
        {
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            Status = SaleStatus.Anulado,
            Total = 118.00m
        };
        var specification = new SaleCanBeCancelledSpecification();

        // Act
        var result = specification.IsSatisfiedBy(sale);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithNullSale_ShouldReturnFalse()
    {
        // Arrange
        Sale? sale = null;
        var specification = new SaleCanBeCancelledSpecification();

        // Act
        var result = specification.IsSatisfiedBy(sale);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ToExpression_ShouldReturnCorrectExpression()
    {
        // Arrange
        var specification = new SaleCanBeCancelledSpecification();
        var sale = new Sale
        {
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            Status = SaleStatus.Pagado,
            Total = 118.00m
        };

        // Act
        var expression = specification.ToExpression();
        var compiled = expression.Compile();
        var result = compiled(sale);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ToExpression_WithAnuladoStatus_ShouldReturnFalse()
    {
        // Arrange
        var specification = new SaleCanBeCancelledSpecification();
        var sale = new Sale
        {
            DocumentNumber = "B001-00000001",
            DocumentType = DocumentType.Boleta,
            Status = SaleStatus.Anulado,
            Total = 118.00m
        };

        // Act
        var expression = specification.ToExpression();
        var compiled = expression.Compile();
        var result = compiled(sale);

        // Assert
        result.Should().BeFalse();
    }
}

