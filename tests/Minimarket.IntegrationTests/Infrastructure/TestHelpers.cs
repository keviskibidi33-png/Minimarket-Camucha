using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Minimarket.Infrastructure.Data;

namespace Minimarket.IntegrationTests.Infrastructure;

public static class TestHelpers
{
    public static async Task<string> GetAuthTokenAsync(HttpClient client, string email = "testuser@minimarket.com", string password = "Test@1234")
    {
        var loginRequest = new { email, password };
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        if (jsonDoc.RootElement.TryGetProperty("data", out var dataElement) &&
            dataElement.TryGetProperty("token", out var tokenElement))
        {
            return tokenElement.GetString() ?? string.Empty;
        }
        
        throw new Exception("No se pudo obtener el token de autenticación");
    }

    public static async Task<Guid> GetFirstCategoryIdAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var categories = JsonSerializer.Deserialize<List<CategoryDto>>(content, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        return categories?.FirstOrDefault()?.Id ?? Guid.Empty;
    }

    public static async Task<Guid> GetFirstProductIdAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        if (jsonDoc.RootElement.TryGetProperty("items", out var itemsElement))
        {
            var products = JsonSerializer.Deserialize<List<ProductDto>>(itemsElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return products?.FirstOrDefault()?.Id ?? Guid.Empty;
        }
        
        throw new Exception("No se encontraron productos");
    }

    public static HttpClient CreateAuthenticatedClient(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public int Stock { get; set; }
}

