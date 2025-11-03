using Microsoft.AspNetCore.Mvc.Testing;
using Minimarket.API;
using Xunit;

namespace Minimarket.IntegrationTests;

public class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    public BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}

