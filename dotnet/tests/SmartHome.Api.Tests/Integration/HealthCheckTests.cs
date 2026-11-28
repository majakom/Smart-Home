using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartHome.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SmartHome.Api.Tests.Integration;

public sealed class HealthCheckTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private const string UnavailableConnectionString =
        "Host=127.0.0.1;Port=1;Database=smarthome_tests;Username=test;Password=test;Timeout=1";

    private readonly ApiFactory _factory;
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18").Build();

    public HealthCheckTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _postgres.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgres.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task GetHealthLive_ReturnsHealthy_EvenWhenPostgresIsUnavailable()
    {
        // Arrange
        using WebApplicationFactory<Program> testFactory =
            CreateFactory(UnavailableConnectionString);
        using HttpClient client = testFactory.CreateClient();

        // Act
        using HttpResponseMessage response = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetHealthReady_ReturnsHealthy_WhenPostgresIsAvailable()
    {
        // Arrange
        using WebApplicationFactory<Program> testFactory =
            CreateFactory(_postgres.GetConnectionString());
        using HttpClient client = testFactory.CreateClient();

        // Act
        using HttpResponseMessage response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetHealthReady_ReturnsServiceUnavailable_WhenPostgresIsUnavailable()
    {
        // Arrange
        using WebApplicationFactory<Program> testFactory =
            CreateFactory(UnavailableConnectionString);
        using HttpClient client = testFactory.CreateClient();

        // Act
        using HttpResponseMessage response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
    }

    private WebApplicationFactory<Program> CreateFactory(string connectionString)
    {
        return _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<SmartHomeDbContext>>();
                services.AddDbContext<SmartHomeDbContext>(options =>
                    options.UseNpgsql(connectionString));
            })
        );
    }
}
