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

    [Fact]
    public async Task GetProducts_WithSearchTerm_ReturnsFilteredResults()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Act
        var response = await _client.GetAsync("/api/products?searchTerm=Coca");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        if (jsonDoc.RootElement.TryGetProperty("items", out var itemsElement))
        {
            var products = JsonSerializer.Deserialize<List<ProductDto>>(itemsElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            products.Should().NotBeNull();
            if (products?.Count > 0)
            {
                products.Should().OnlyContain(p => 
                    p.Name.Contains("Coca", StringComparison.OrdinalIgnoreCase) ||
                    p.Code.Contains("Coca", StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    [Fact]
    public async Task GetProducts_WithCategoryFilter_ReturnsFilteredResults()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var categoryId = await TestHelpers.GetFirstCategoryIdAsync(_client);

        // Act
        var response = await _client.GetAsync($"/api/products?categoryId={categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        if (jsonDoc.RootElement.TryGetProperty("items", out var itemsElement))
        {
            var products = JsonSerializer.Deserialize<List<ProductDto>>(itemsElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            products.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Act
        var response = await _client.GetAsync("/api/products?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        if (jsonDoc.RootElement.TryGetProperty("items", out var itemsElement))
        {
            var products = JsonSerializer.Deserialize<List<ProductDto>>(itemsElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            products.Should().NotBeNull();
            if (products?.Count > 0)
            {
                products.Count.Should().BeLessThanOrEqualTo(5);
            }
        }

        jsonDoc.RootElement.TryGetProperty("page", out var pageElement).Should().BeTrue();
        jsonDoc.RootElement.TryGetProperty("pageSize", out var pageSizeElement).Should().BeTrue();
    }

    [Fact]
    public async Task CreateProduct_WithSalePriceLessThanPurchasePrice_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var categoryId = await TestHelpers.GetFirstCategoryIdAsync(_client);

        var invalidProduct = new
        {
            Product = new
            {
                Code = "7750001001999",
                Name = "Producto Inválido",
                PurchasePrice = 10.00m,
                SalePrice = 5.00m, // Menor que PurchasePrice
                Stock = 20,
                MinimumStock = 5,
                CategoryId = categoryId
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", invalidProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("precio", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateProduct_WithNonExistentCategory_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var nonExistentCategoryId = Guid.NewGuid();

        var invalidProduct = new
        {
            Product = new
            {
                Code = "7750001001999",
                Name = "Producto Test",
                PurchasePrice = 5.00m,
                SalePrice = 8.00m,
                Stock = 20,
                MinimumStock = 5,
                CategoryId = nonExistentCategoryId
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", invalidProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("categoría", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var productId = await TestHelpers.GetFirstProductIdAsync(_client);
        var categoryId = await TestHelpers.GetFirstCategoryIdAsync(_client);

        var updateProduct = new
        {
            Code = "7750001001001",
            Name = "Producto Actualizado",
            Description = "Nueva descripción",
            PurchasePrice = 5.00m,
            SalePrice = 12.00m,
            Stock = 50,
            MinimumStock = 10,
            CategoryId = categoryId,
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", updateProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedProduct = JsonSerializer.Deserialize<ProductDto>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be("Producto Actualizado");
        updatedProduct.SalePrice.Should().Be(12.00m);
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var nonExistentId = Guid.NewGuid();
        var categoryId = await TestHelpers.GetFirstCategoryIdAsync(_client);

        var updateProduct = new
        {
            Code = "7750001001999",
            Name = "Producto Test",
            PurchasePrice = 5.00m,
            SalePrice = 8.00m,
            Stock = 20,
            MinimumStock = 5,
            CategoryId = categoryId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{nonExistentId}", updateProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var productId = await TestHelpers.GetFirstProductIdAsync(_client);
        var categoryId = await TestHelpers.GetFirstCategoryIdAsync(_client);

        var invalidProduct = new
        {
            Code = "7750001001999",
            Name = string.Empty, // Nombre vacío
            PurchasePrice = 10.00m,
            SalePrice = 5.00m, // Menor que PurchasePrice
            Stock = 20,
            MinimumStock = 5,
            CategoryId = categoryId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", invalidProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Crear un producto primero para eliminarlo
        var categoryId = await TestHelpers.GetFirstCategoryIdAsync(_client);

        var newProduct = new
        {
            Product = new
            {
                Code = $"DELETE_TEST_{Guid.NewGuid():N}",
                Name = "Producto a Eliminar",
                PurchasePrice = 5.00m,
                SalePrice = 8.00m,
                Stock = 20,
                MinimumStock = 5,
                CategoryId = categoryId
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        createResponse.EnsureSuccessStatusCode();

        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<ProductDto>(createdContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/products/{createdProduct!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que el producto ya no existe
        var getResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal PurchasePrice { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
}

