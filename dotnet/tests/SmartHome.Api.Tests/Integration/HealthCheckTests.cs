
using System.Net;
using System.Net.Http.Json;

namespace SmartHome.Api.Tests.Integration;

public sealed class HealthCheckTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task GetHealth_ReturnsHealthy()
    {
        // Arrange
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "application/json",
            response.Content.Headers.ContentType?.MediaType
        );

        var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(result);
        Assert.Equal("Healthy", result.Status);
    }

    private sealed record HealthResponse(string Status);
}
