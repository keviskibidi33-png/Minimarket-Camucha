using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Minimarket.IntegrationTests.Infrastructure;
using Xunit;

namespace Minimarket.FunctionalTests.Flows;

[Collection("IntegrationTests")]
public class SaleFlowTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SaleFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteSaleFlow_CreateSale_UpdatesStockAndGeneratesInvoice()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Obtener producto con stock
        var productId = await TestHelpers.GetFirstProductIdAsync(_client);

        // Obtener stock inicial
        var productResponse = await _client.GetAsync($"/api/products/{productId}");
        var productContent = await productResponse.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(productContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var initialStock = product!.Stock;

        // Crear venta
        var createSaleCommand = new
        {
            Sale = new
            {
                DocumentType = 1, // Boleta
                PaymentMethod = 1, // Efectivo
                AmountPaid = 20.00m,
                SaleDetails = new[]
                {
                    new
                    {
                        ProductId = productId,
                        Quantity = 3,
                        UnitPrice = product.SalePrice
                    }
                }
            }
        };

        // Act - Crear venta
        var createResponse = await _client.PostAsJsonAsync("/api/sales", createSaleCommand);

        // Assert - Venta creada exitosamente
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var saleContent = await createResponse.Content.ReadAsStringAsync();
        var sale = JsonSerializer.Deserialize<SaleDto>(saleContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        sale.Should().NotBeNull();
        sale!.DocumentNumber.Should().MatchRegex(@"^B\d{3}-\d{8}$");
        sale.Total.Should().BeGreaterThan(0);

        // Assert - Stock actualizado
        var updatedProductResponse = await _client.GetAsync($"/api/products/{productId}");
        var updatedProductContent = await updatedProductResponse.Content.ReadAsStringAsync();
        var updatedProduct = JsonSerializer.Deserialize<ProductDto>(updatedProductContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        updatedProduct!.Stock.Should().Be(initialStock - 3);

        // Assert - Venta aparece en listado
        var salesResponse = await _client.GetAsync("/api/sales");
        salesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateSale_WithInsufficientStock_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var productId = await TestHelpers.GetFirstProductIdAsync(_client);

        var createSaleCommand = new
        {
            Sale = new
            {
                DocumentType = 1,
                PaymentMethod = 1,
                AmountPaid = 1000.00m,
                SaleDetails = new[]
                {
                    new
                    {
                        ProductId = productId,
                        Quantity = 10000, // Más del stock disponible
                        UnitPrice = 3.50m
                    }
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", createSaleCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Stock", StringComparison.OrdinalIgnoreCase);
    }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public int Stock { get; set; }
    public decimal SalePrice { get; set; }
}

public class SaleDto
{
    public Guid Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

