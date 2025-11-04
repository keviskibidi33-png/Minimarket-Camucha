using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Minimarket.IntegrationTests.Infrastructure;
using Xunit;

namespace Minimarket.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class CustomersControllerTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CustomersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_WithAuthentication_ReturnsSuccessStatusCode()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        jsonDoc.RootElement.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCustomers_WithSearchTerm_ReturnsFilteredResults()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Act
        var response = await _client.GetAsync("/api/customers?searchTerm=Juan");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        if (jsonDoc.RootElement.TryGetProperty("items", out var itemsElement))
        {
            var customers = JsonSerializer.Deserialize<List<CustomerDto>>(itemsElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            customers.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetCustomers_WithDocumentTypeFilter_ReturnsFilteredResults()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Act
        var response = await _client.GetAsync("/api/customers?documentType=DNI");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        jsonDoc.RootElement.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCustomers_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Act
        var response = await _client.GetAsync("/api/customers?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        if (jsonDoc.RootElement.TryGetProperty("items", out var itemsElement))
        {
            var customers = JsonSerializer.Deserialize<List<CustomerDto>>(itemsElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            customers.Should().NotBeNull();
            if (customers?.Count > 0)
            {
                customers.Count.Should().BeLessThanOrEqualTo(5);
            }
        }

        jsonDoc.RootElement.TryGetProperty("page", out var pageElement).Should().BeTrue();
        jsonDoc.RootElement.TryGetProperty("pageSize", out var pageSizeElement).Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomers_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomerById_WithValidId_ReturnsOk()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        // Crear un cliente primero
        var customerId = await CreateTestCustomerAsync();

        // Act
        var response = await _client.GetAsync($"/api/customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<CustomerDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        customer.Should().NotBeNull();
        customer!.Id.Should().Be(customerId);
    }

    [Fact]
    public async Task GetCustomerById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var newCustomer = new
        {
            Customer = new
            {
                DocumentType = "DNI",
                DocumentNumber = GenerateUniqueDNI(),
                Name = "Juan Pérez Test",
                Phone = "987654321",
                Email = "juan.test@example.com",
                Address = "Av. Test 123"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", newCustomer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdCustomer = JsonSerializer.Deserialize<CustomerDto>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        createdCustomer.Should().NotBeNull();
        createdCustomer!.Name.Should().Be("Juan Pérez Test");
        createdCustomer.DocumentNumber.Should().Be(newCustomer.Customer.DocumentNumber);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidDNI_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var invalidCustomer = new
        {
            Customer = new
            {
                DocumentType = "DNI",
                DocumentNumber = "1234567", // 7 dígitos (debe ser 8)
                Name = "Juan Pérez",
                Phone = "987654321"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", invalidCustomer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("DNI", StringComparison.OrdinalIgnoreCase)
            .Or.Contain("8 dígitos", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidRUC_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var invalidCustomer = new
        {
            Customer = new
            {
                DocumentType = "RUC",
                DocumentNumber = "2012345678", // 10 dígitos (debe ser 11)
                Name = "Empresa Test",
                Phone = "987654321"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", invalidCustomer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("RUC", StringComparison.OrdinalIgnoreCase)
            .Or.Contain("11 dígitos", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateDocument_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var documentNumber = GenerateUniqueDNI();

        var firstCustomer = new
        {
            Customer = new
            {
                DocumentType = "DNI",
                DocumentNumber = documentNumber,
                Name = "Primer Cliente",
                Phone = "987654321"
            }
        };

        // Crear primer cliente
        var firstResponse = await _client.PostAsJsonAsync("/api/customers", firstCustomer);
        firstResponse.EnsureSuccessStatusCode();

        // Intentar crear cliente duplicado
        var duplicateCustomer = new
        {
            Customer = new
            {
                DocumentType = "DNI",
                DocumentNumber = documentNumber, // Mismo documento
                Name = "Segundo Cliente",
                Phone = "987654322"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", duplicateCustomer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("documento", StringComparison.OrdinalIgnoreCase)
            .Or.Contain("existe", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidPhone_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var invalidCustomer = new
        {
            Customer = new
            {
                DocumentType = "DNI",
                DocumentNumber = GenerateUniqueDNI(),
                Name = "Juan Pérez",
                Phone = "81234567" // No empieza con 9
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", invalidCustomer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("teléfono", StringComparison.OrdinalIgnoreCase)
            .Or.Contain("peruano", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var customerId = await CreateTestCustomerAsync();

        var updateCustomer = new
        {
            DocumentType = "DNI",
            DocumentNumber = GenerateUniqueDNI(),
            Name = "Juan Pérez Actualizado",
            Phone = "987654321",
            Email = "juan.updated@example.com"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{customerId}", updateCustomer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedCustomer = JsonSerializer.Deserialize<CustomerDto>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        updatedCustomer.Should().NotBeNull();
        updatedCustomer!.Name.Should().Be("Juan Pérez Actualizado");
    }

    [Fact]
    public async Task UpdateCustomer_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var nonExistentId = Guid.NewGuid();

        var updateCustomer = new
        {
            DocumentType = "DNI",
            DocumentNumber = GenerateUniqueDNI(),
            Name = "Cliente Test",
            Phone = "987654321"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{nonExistentId}", updateCustomer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCustomer_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var customerId = await CreateTestCustomerAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que el cliente ya no existe
        var getResponse = await _client.GetAsync($"/api/customers/{customerId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomer_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var token = await TestHelpers.GetAuthTokenAsync(_client);
        TestHelpers.CreateAuthenticatedClient(_client, token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/customers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Helper methods
    private async Task<Guid> CreateTestCustomerAsync()
    {
        var newCustomer = new
        {
            Customer = new
            {
                DocumentType = "DNI",
                DocumentNumber = GenerateUniqueDNI(),
                Name = "Cliente Test",
                Phone = "987654321",
                Email = "test@example.com"
            }
        };

        var response = await _client.PostAsJsonAsync("/api/customers", newCustomer);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<CustomerDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return customer!.Id;
    }

    private string GenerateUniqueDNI()
    {
        var random = new Random();
        return random.Next(10000000, 99999999).ToString();
    }
}

public class CustomerDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

