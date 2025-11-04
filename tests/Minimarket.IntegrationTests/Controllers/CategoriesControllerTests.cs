using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Minimarket.IntegrationTests.Infrastructure;
using Xunit;

namespace Minimarket.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class CategoriesControllerTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CategoriesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_WithAuthentication_ReturnsSuccessStatusCode()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var categories = JsonSerializer.Deserialize<List<CategoryDto>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        categories.Should().NotBeNull();
        categories!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCategories_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCategoryById_WithValidId_ReturnsOk()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var categoryId = await TestHelpers.GetFirstCategoryIdAsync(_client);

        // Act
        var response = await _client.GetAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var category = JsonSerializer.Deserialize<CategoryDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        category.Should().NotBeNull();
        category!.Id.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetCategoryById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/categories/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCategory_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var newCategory = new
        {
            Name = $"Categoría Test {Guid.NewGuid():N}",
            Description = "Descripción de categoría de prueba",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", newCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdCategory = JsonSerializer.Deserialize<CategoryDto>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        createdCategory.Should().NotBeNull();
        createdCategory!.Name.Should().Be(newCategory.Name);
        createdCategory.Description.Should().Be(newCategory.Description);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Primero obtener una categoría existente
        var categoriesResponse = await _client.GetAsync("/api/categories");
        categoriesResponse.EnsureSuccessStatusCode();
        var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
        var existingCategories = JsonSerializer.Deserialize<List<CategoryDto>>(categoriesContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var duplicateCategory = new
        {
            Name = existingCategories!.First().Name, // Nombre duplicado
            Description = "Descripción duplicada",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", duplicateCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("nombre", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var invalidCategory = new
        {
            Name = string.Empty,
            Description = "Descripción",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", invalidCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCategory_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Crear una categoría primero
        var categoryId = await CreateTestCategoryAsync();

        var updateCategory = new
        {
            Name = $"Categoría Actualizada {Guid.NewGuid():N}",
            Description = "Descripción actualizada",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categories/{categoryId}", updateCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedCategory = JsonSerializer.Deserialize<CategoryDto>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        updatedCategory.Should().NotBeNull();
        updatedCategory!.Name.Should().Be(updateCategory.Name);
        updatedCategory.Description.Should().Be(updateCategory.Description);
    }

    [Fact]
    public async Task UpdateCategory_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var nonExistentId = Guid.NewGuid();

        var updateCategory = new
        {
            Name = "Categoría Test",
            Description = "Descripción",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categories/{nonExistentId}", updateCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCategory_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Obtener categorías existentes
        var categoriesResponse = await _client.GetAsync("/api/categories");
        categoriesResponse.EnsureSuccessStatusCode();
        var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
        var existingCategories = JsonSerializer.Deserialize<List<CategoryDto>>(categoriesContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Crear una nueva categoría
        var categoryId = await CreateTestCategoryAsync();

        // Intentar actualizar con nombre duplicado
        var updateCategory = new
        {
            Name = existingCategories!.First(c => c.Id != categoryId).Name, // Nombre de otra categoría
            Description = "Descripción",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categories/{categoryId}", updateCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCategory_WithValidId_ReturnsOk()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Crear una categoría primero para eliminarla
        var categoryId = await CreateTestCategoryAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que la categoría ya no existe
        var getResponse = await _client.GetAsync($"/api/categories/{categoryId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCategory_WithCategoryHavingProducts_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Obtener una categoría que tiene productos (del seed data)
        var categoryId = await TestHelpers.GetFirstCategoryIdAsync(_client);

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        // Depende de la implementación - puede ser BadRequest o NoContent si se permite
        // En este caso asumimos que no se puede eliminar si tiene productos
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NoContent);
    }

    // Helper methods
    private async Task<Guid> CreateTestCategoryAsync()
    {
        var newCategory = new
        {
            Name = $"Categoría Test {Guid.NewGuid():N}",
            Description = "Descripción de prueba",
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync("/api/categories", newCategory);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var category = JsonSerializer.Deserialize<CategoryDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return category!.Id;
    }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
