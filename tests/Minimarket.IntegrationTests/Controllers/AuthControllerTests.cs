using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Minimarket.IntegrationTests.Infrastructure;
using Xunit;

namespace Minimarket.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class AuthControllerTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "testuser@minimarket.com",
            Password = "Test@1234"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        jsonDoc.RootElement.TryGetProperty("data", out var dataElement).Should().BeTrue();
        dataElement.TryGetProperty("token", out var tokenElement).Should().BeTrue();
        tokenElement.GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "testuser@minimarket.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "nonexistent@example.com",
            Password = "Password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

