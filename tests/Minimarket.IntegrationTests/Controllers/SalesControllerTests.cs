using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Minimarket.IntegrationTests.Infrastructure;
using Xunit;

namespace Minimarket.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class SalesControllerTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SalesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSale_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var productId = await TestHelpers.GetFirstProductIdAsync(_client);

        var createSaleCommand = new
        {
            Sale = new
            {
                DocumentType = 1, // Boleta
                PaymentMethod = 1, // Efectivo
                AmountPaid = 25.00m,
                Discount = 0m,
                SaleDetails = new[]
                {
                    new
                    {
                        ProductId = productId,
                        Quantity = 5,
                        UnitPrice = 3.50m
                    }
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", createSaleCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var sale = JsonSerializer.Deserialize<SaleDto>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        sale.Should().NotBeNull();
        sale!.DocumentNumber.Should().MatchRegex(@"^B\d{3}-\d{8}$");
        sale.Total.Should().BeGreaterThan(0);
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

    [Fact]
    public async Task CreateSale_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var createSaleCommand = new
        {
            Sale = new
            {
                DocumentType = 1,
                PaymentMethod = 1,
                AmountPaid = 25.00m,
                SaleDetails = new[]
                {
                    new
                    {
                        ProductId = Guid.NewGuid(),
                        Quantity = 5,
                        UnitPrice = 3.50m
                    }
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", createSaleCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllSales_WithAuthentication_ReturnsOk()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Act
        var response = await _client.GetAsync("/api/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class SaleDto
{
    public Guid Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

