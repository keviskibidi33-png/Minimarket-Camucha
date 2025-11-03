using FluentAssertions;
using Minimarket.Domain.Entities;
using Xunit;

namespace Minimarket.UnitTests.Domain.Entities;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCategory()
    {
        // Arrange & Act
        var category = new Category
        {
            Name = "Bebidas",
            Description = "Categoría de bebidas",
            IsActive = true
        };

        // Assert
        category.Should().NotBeNull();
        category.Name.Should().Be("Bebidas");
        category.Description.Should().Be("Categoría de bebidas");
        category.IsActive.Should().BeTrue();
        category.Id.Should().NotBeEmpty();
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldAllowEmptyDescription()
    {
        // Arrange & Act
        var category = new Category
        {
            Name = "Snacks",
            Description = string.Empty,
            IsActive = true
        };

        // Assert
        category.Description.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_ActiveCategory_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var category = new Category
        {
            Name = "Bebidas",
            IsActive = true
        };

        // Act
        category.IsActive = false;

        // Assert
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var category = new Category
        {
            Name = "Bebidas Antiguas"
        };

        // Act
        category.Name = "Bebidas Nuevas";

        // Assert
        category.Name.Should().Be("Bebidas Nuevas");
    }
}

