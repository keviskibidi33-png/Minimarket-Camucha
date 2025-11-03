using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Minimarket.IntegrationTests.Infrastructure;
using Xunit;

namespace Minimarket.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class ProductsControllerTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_WithAuthentication_ReturnsSuccessStatusCode()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        jsonDoc.RootElement.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProducts_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var categoryId = await TestHelpers.GetFirstCategoryIdAsync(_client);

        var newProduct = new
        {
            Product = new
            {
                Code = "7750001001999",
                Name = "Producto de Prueba",
                PurchasePrice = 5.00m,
                SalePrice = 8.00m,
                Stock = 20,
                MinimumStock = 5,
                CategoryId = categoryId,
                Description = "Descripción del producto"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<ProductDto>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be("Producto de Prueba");
        createdProduct.SalePrice.Should().Be(8.00m);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var categoryId = await TestHelpers.GetFirstCategoryIdAsync(_client);

        var duplicateProduct = new
        {
            Product = new
            {
                Code = "7750001001001", // Código ya existe en seed data
                Name = "Producto Duplicado",
                PurchasePrice = 5.00m,
                SalePrice = 8.00m,
                Stock = 10,
                MinimumStock = 5,
                CategoryId = categoryId
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", duplicateProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("código", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ReturnsOk()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var productId = await TestHelpers.GetFirstProductIdAsync(_client);

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        product.Should().NotBeNull();
        product!.Id.Should().Be(productId);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
}

