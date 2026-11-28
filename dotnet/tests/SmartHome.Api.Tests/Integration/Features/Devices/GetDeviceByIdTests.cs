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

public class GetDeviceByIdTests : IClassFixture<ApiFactory>, IDisposable
{
    private readonly WebApplicationFactory<Program> _testFactory;
    private readonly IDeviceReader _deviceReader;
    private readonly HttpClient _client;

    public GetDeviceByIdTests(ApiFactory factory)
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
    public async Task GetDeviceById_ReturnsOkWithDevice_WhenDeviceExists()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");

        DateTime lastSeenAtUtc = DateTime.UtcNow;

        device.MarkAsSeen(lastSeenAtUtc);
        device.Disable();

        _deviceReader.GetByIdAsync(
            device.Id,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Device?>(device));

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            $"/api/devices/{device.Id.Value}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument json = JsonDocument.Parse(content);

        JsonElement result = json.RootElement;

        Assert.Equal(device.Id.Value, result.GetProperty("id").GetGuid());
        Assert.Equal("Device Name", result.GetProperty("name").GetString());
        Assert.Equal("Device Model", result.GetProperty("model").GetString());
        Assert.Equal(device.RegisteredAtUtc, result.GetProperty("registeredAtUtc").GetDateTime());
        Assert.Equal(lastSeenAtUtc, result.GetProperty("lastSeenAtUtc").GetDateTime());
        Assert.False(result.GetProperty("isEnabled").GetBoolean());

        await _deviceReader.Received(1).GetByIdAsync(
            device.Id,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetDeviceById_ReturnsNotFound_WhenDeviceDoesNotExist()
    {
        // Arrange
        DeviceId deviceId = DeviceId.New();

        _deviceReader.GetByIdAsync(
            deviceId,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Device?>(null));

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            $"/api/devices/{deviceId.Value}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await _deviceReader.Received(1).GetByIdAsync(
            deviceId,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetDeviceById_ReturnsBadRequest_WhenDeviceIdIsInvalid()
    {
        // Arrange
        string invalidDeviceId = "invalid-guid";

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            $"/api/devices/{invalidDeviceId}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType
        );

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument json = JsonDocument.Parse(content);

        JsonElement result = json.RootElement;

        Assert.Equal(JsonValueKind.Object, result.ValueKind);

        JsonElement errors = result.GetProperty("errors");
        JsonElement deviceIdErrors = errors.GetProperty("deviceId");
       
        Assert.Equal(JsonValueKind.Array, deviceIdErrors.ValueKind);
        Assert.Equal(
            "The device ID must be a valid GUID.",
            Assert.Single(deviceIdErrors.EnumerateArray()).GetString()
        );

        await _deviceReader.DidNotReceive().GetByIdAsync(
            Arg.Any<DeviceId>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetDeviceById_ReturnsInternalServerError_WhenUseCaseThrows()
    {
        // Arrange
        DeviceId deviceId = DeviceId.New();

        _deviceReader.GetByIdAsync(
            Arg.Any<DeviceId>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromException<Device?>(
            new InvalidOperationException("Simulated failure.")
        ));

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            $"/api/devices/{deviceId.Value}"
        );

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

        await _deviceReader.Received(1).GetByIdAsync(
            Arg.Any<DeviceId>(),
            Arg.Any<CancellationToken>()
        );
    }

    public void Dispose()
    {
        _client.Dispose();
        _testFactory.Dispose();
    }
}
