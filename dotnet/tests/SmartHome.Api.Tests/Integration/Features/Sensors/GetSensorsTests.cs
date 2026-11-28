using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;
using SmartHome.Domain.Shared;

namespace SmartHome.Api.Tests.Integration.Features.Sensors;

public class GetSensorsTests : IClassFixture<ApiFactory>, IDisposable
{
    private readonly WebApplicationFactory<Program> _testFactory;
    private readonly ISensorReader _sensorReader;
    private readonly HttpClient _client;

    public GetSensorsTests(ApiFactory factory)
    {
        _sensorReader = Substitute.For<ISensorReader>();

        _testFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ISensorReader>();
                services.AddSingleton(_sensorReader);
            });
        });

        _client = _testFactory.CreateClient();
    }

    [Fact]
    public async Task GetSensors_ReturnsOkWithSensors()
    {
        // Arrange
        Sensor firstSensor = new Sensor(DeviceId.New(), SensorCategory.Climate, "Sensor Name A", "Sensor Model A", "SN12345");
        Sensor secondSensor = new Sensor(DeviceId.New(), SensorCategory.Climate, "Sensor Name B", "Sensor Model B");

        secondSensor.Disable();

        IReadOnlyList<Sensor> sensors = [firstSensor, secondSensor];

        _sensorReader.GetAllAsync(
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(sensors));

        // Act
        using HttpResponseMessage response = await _client.GetAsync("/api/sensors");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument json = JsonDocument.Parse(content);

        JsonElement result = json.RootElement;

        Assert.Equal(JsonValueKind.Array, result.ValueKind);
        Assert.Equal(2, result.GetArrayLength());

        JsonElement first = result[0];
        Assert.Equal(firstSensor.Id.Value, first.GetProperty("id").GetGuid());
        Assert.Equal(firstSensor.DeviceId.Value, first.GetProperty("deviceId").GetGuid());
        Assert.Equal("Climate", first.GetProperty("category").GetString());
        Assert.Equal("Sensor Name A", first.GetProperty("name").GetString());
        Assert.Equal("Sensor Model A", first.GetProperty("model").GetString());
        Assert.Equal("SN12345", first.GetProperty("serialNumber").GetString());
        Assert.Equal(firstSensor.RegisteredAtUtc, first.GetProperty("registeredAtUtc").GetDateTime());
        Assert.True(first.GetProperty("isEnabled").GetBoolean());

        JsonElement second = result[1];
        Assert.Equal(secondSensor.Id.Value, second.GetProperty("id").GetGuid());
        Assert.Equal(secondSensor.DeviceId.Value, second.GetProperty("deviceId").GetGuid());
        Assert.Equal("Climate", second.GetProperty("category").GetString());
        Assert.Equal("Sensor Name B", second.GetProperty("name").GetString());
        Assert.Equal("Sensor Model B", second.GetProperty("model").GetString());
        Assert.Null(second.GetProperty("serialNumber").GetString());
        Assert.Equal(secondSensor.RegisteredAtUtc, second.GetProperty("registeredAtUtc").GetDateTime());
        Assert.False(second.GetProperty("isEnabled").GetBoolean());

        await _sensorReader.Received(1).GetAllAsync(
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSensors_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        _sensorReader.GetAllAsync(
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<IReadOnlyList<Sensor>>([]));

        // Act
        using HttpResponseMessage response = await _client.GetAsync("/api/sensors");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument json = JsonDocument.Parse(content);

        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        Assert.Empty(json.RootElement.EnumerateArray());

        await _sensorReader.Received(1).GetAllAsync(
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSensors_ReturnsInternalServerError_WhenUseCaseThrows()
    {
        // Arrange
        _sensorReader.GetAllAsync(
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromException<IReadOnlyList<Sensor>>(
            new InvalidOperationException("Simulated failure.")
        ));

        // Act
        using HttpResponseMessage response = await _client.GetAsync("/api/sensors");

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

        await _sensorReader.Received(1).GetAllAsync(
            Arg.Any<CancellationToken>()
        );
    }

    public void Dispose()
    {
        _client.Dispose();
        _testFactory.Dispose();
    }
}
