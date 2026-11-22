
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Domain.Devices;

namespace SmartHome.Api.Tests.Integration.Features.Devices;

public class GetDevicesTests : IClassFixture<ApiFactory>, IDisposable
{
    private readonly WebApplicationFactory<Program> _testFactory;
    private readonly IDeviceReader _deviceReader;
    private readonly HttpClient _client;

    public GetDevicesTests(ApiFactory factory)
    {
        _deviceReader = Substitute.For<IDeviceReader>();

        _testFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDeviceReader>();
                services.AddSingleton(_deviceReader);
            });
        });

        _client = _testFactory.CreateClient();
    }

    [Fact]
    public async Task GetDevices_ReturnsOkWithDevices()
    {
        // Arrange
        _deviceReader.ClearReceivedCalls();

        Device firstDevice = new("Device Name A", "Device Model A");
        Device secondDevice = new("Device Name B", "Device Model B");

        DateTime lastSeenAtUtc = DateTime.UtcNow;

        secondDevice.MarkAsSeen(lastSeenAtUtc);
        secondDevice.Disable();

        IReadOnlyList<Device> devices = [firstDevice, secondDevice];

        _deviceReader.GetAllAsync(
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(devices));

        // Act
        using HttpResponseMessage response = await _client.GetAsync("/api/devices");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument json = JsonDocument.Parse(content);

        JsonElement result = json.RootElement;

        Assert.Equal(JsonValueKind.Array, result.ValueKind);
        Assert.Equal(2, result.GetArrayLength());

        JsonElement first = result[0];
        Assert.Equal(firstDevice.Id.Value, first.GetProperty("id").GetGuid());
        Assert.Equal("Device Name A", first.GetProperty("name").GetString());
        Assert.Equal("Device Model A", first.GetProperty("model").GetString());
        Assert.Equal(firstDevice.RegisteredAtUtc, first.GetProperty("registeredAtUtc").GetDateTime());
        Assert.Equal(JsonValueKind.Null, first.GetProperty("lastSeenAtUtc").ValueKind);
        Assert.True(first.GetProperty("isEnabled").GetBoolean());

        JsonElement second = result[1];
        Assert.Equal(secondDevice.Id.Value, second.GetProperty("id").GetGuid());
        Assert.Equal("Device Name B", second.GetProperty("name").GetString());
        Assert.Equal("Device Model B", second.GetProperty("model").GetString());
        Assert.Equal(secondDevice.RegisteredAtUtc, second.GetProperty("registeredAtUtc").GetDateTime());
        Assert.Equal(lastSeenAtUtc, second.GetProperty("lastSeenAtUtc").GetDateTime());
        Assert.False(second.GetProperty("isEnabled").GetBoolean());

        await _deviceReader.Received(1).GetAllAsync(
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetDevices_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        _deviceReader.ClearReceivedCalls();

        _deviceReader.GetAllAsync(
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<IReadOnlyList<Device>>([]));

        // Act
        using HttpResponseMessage response =
            await _client.GetAsync("/api/devices");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument json = JsonDocument.Parse(content);

        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        Assert.Empty(json.RootElement.EnumerateArray());

        await _deviceReader.Received(1).GetAllAsync(
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetDevices_ReturnsInternalServerError_WhenUseCaseThrows()
    {
        // Arrange
        _deviceReader.ClearReceivedCalls();

        _deviceReader.GetAllAsync(
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromException<IReadOnlyList<Device>>(
            new InvalidOperationException("Simulated failure.")
        ));

        // Act
        using HttpResponseMessage response =
            await _client.GetAsync("/api/devices");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType
        );

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument json = JsonDocument.Parse(content);

        Assert.Equal((int)HttpStatusCode.InternalServerError, json.RootElement.GetProperty("status").GetInt32());

        // Internal exception details should not leak into the response.
        Assert.DoesNotContain("Simulated failure", content);

        await _deviceReader.Received(1).GetAllAsync(
            Arg.Any<CancellationToken>()
        );
    }

    public void Dispose()
    {
        _client.Dispose();
        _testFactory.Dispose();
    }
}
