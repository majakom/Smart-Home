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

namespace SmartHome.Api.Tests.Integration.Features.Sensors;

public class GetSensorByIdTests : IClassFixture<ApiFactory>, IDisposable
{
    private readonly WebApplicationFactory<Program> _testFactory;
    private readonly ISensorReader _sensorReader;
    private readonly HttpClient _client;

    public GetSensorByIdTests(ApiFactory factory)
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
    public async Task GetSensorById_ReturnsOkWithSensor_WhenSensorExists()
    {
        // Arrange
        Sensor sensor = new Sensor(DeviceId.New(), SensorCategory.Climate, "Sensor Name", "Sensor Model", "SN12345");

        sensor.Disable();


        _sensorReader.GetByIdAsync(
            sensor.Id,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(sensor));

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            $"/api/sensors/{sensor.Id.Value}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument json = JsonDocument.Parse(content);

        JsonElement result = json.RootElement;

        Assert.Equal(sensor.Id.Value, result.GetProperty("id").GetGuid());
        Assert.Equal(sensor.DeviceId.Value, result.GetProperty("deviceId").GetGuid());
        Assert.Equal("Climate", result.GetProperty("category").GetString());
        Assert.Equal("Sensor Name", result.GetProperty("name").GetString());
        Assert.Equal("Sensor Model", result.GetProperty("model").GetString());
        Assert.Equal("SN12345", result.GetProperty("serialNumber").GetString());
        Assert.Equal(sensor.RegisteredAtUtc, result.GetProperty("registeredAtUtc").GetDateTime());
        Assert.False(result.GetProperty("isEnabled").GetBoolean());

        await _sensorReader.Received(1).GetByIdAsync(
            sensor.Id,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSensorById_ReturnsNotFound_WhenSensorDoesNotExist()
    {
        // Arrange
        SensorId sensorId = SensorId.New();

        _sensorReader.GetByIdAsync(
            sensorId,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(null));

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            $"/api/sensors/{sensorId.Value}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await _sensorReader.Received(1).GetByIdAsync(
            sensorId,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSensorById_ReturnsBadRequest_WhenSensorIdIsInvalid()
    {
        // Arrange
        string invalidSensorId = "invalid-guid";

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            $"/api/sensors/{invalidSensorId}"
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
        JsonElement sensorIdErrors = errors.GetProperty("sensorId");

        Assert.Equal(JsonValueKind.Array, sensorIdErrors.ValueKind);
        Assert.Equal(
            "The sensor ID must be a valid GUID.",
            Assert.Single(sensorIdErrors.EnumerateArray()).GetString()
        );

        await _sensorReader.DidNotReceive().GetByIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSensorById_ReturnsInternalServerError_WhenUseCaseThrows()
    {
        // Arrange
        SensorId sensorId = SensorId.New();

        _sensorReader.GetByIdAsync(
            sensorId,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromException<Sensor?>(
            new InvalidOperationException("Simulated failure.")
        ));

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            $"/api/sensors/{sensorId.Value}"
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

        await _sensorReader.Received(1).GetByIdAsync(
            sensorId,
            Arg.Any<CancellationToken>()
        );
    }

    public void Dispose()
    {
        _client.Dispose();
        _testFactory.Dispose();
    }
}