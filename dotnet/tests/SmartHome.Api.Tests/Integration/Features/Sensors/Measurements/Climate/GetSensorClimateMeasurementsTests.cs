using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Api.Tests.Integration.Features.Sensors.Measurements.Climate;

public class GetSensorClimateMeasurementsTests : IClassFixture<ApiFactory>, IDisposable
{
    private readonly WebApplicationFactory<Program> _testFactory;
    private readonly ISensorReader _sensorReader;
    private readonly IClimateMeasurementReader _measurementReader;
    private readonly HttpClient _client;

    public GetSensorClimateMeasurementsTests(ApiFactory factory)
    {
        _sensorReader = Substitute.For<ISensorReader>();
        _measurementReader = Substitute.For<IClimateMeasurementReader>();

        _testFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ISensorReader>();
                services.RemoveAll<IClimateMeasurementReader>();

                services.AddSingleton(_sensorReader);
                services.AddSingleton(_measurementReader);
            });
        });

        _client = _testFactory.CreateClient();
    }

    [Fact]
    public async Task GetSensorClimateMeasurements_ReturnsOkWithMeasurements_WhenMeasurementsExist()
    {
        // Arrange
        Sensor sensor = CreateSensor();

        DateTime fromUtc = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

        ClimateMeasurement firstMeasurement = CreateMeasurement(
            new SourceMessageId(1),
            sensor.Id,
            fromUtc.AddMinutes(10),
            fromUtc.AddMinutes(11),
            25.0m,
            50.0m
        );

        ClimateMeasurement secondMeasurement = CreateMeasurement(
            new SourceMessageId(2),
            sensor.Id,
            fromUtc.AddMinutes(20),
            fromUtc.AddMinutes(21),
            26.0m,
            51.0m
        );

        IReadOnlyList<ClimateMeasurement> measurements = [firstMeasurement, secondMeasurement];

        _sensorReader.GetByIdAsync(
            sensor.Id,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(sensor));

        _measurementReader.GetBySensorIdAsync(
            sensor.Id,
            fromUtc,
            toUtc,
            100,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(measurements));

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            BuildUrl(sensor.Id.Value, fromUtc, toUtc)
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument json = JsonDocument.Parse(content);

        JsonElement result = json.RootElement;

        Assert.Equal(JsonValueKind.Array, result.ValueKind);
        Assert.Equal(2, result.GetArrayLength());

        JsonElement first = result[0];

        Assert.Equal(firstMeasurement.Id.Value, first.GetProperty("id").GetGuid());
        Assert.Equal(1, first.GetProperty("sourceMessageId").GetInt64());
        Assert.Equal(sensor.Id.Value, first.GetProperty("sensorId").GetGuid());
        Assert.Equal(firstMeasurement.MeasuredAtUtc, first.GetProperty("measuredAtUtc").GetDateTime());
        Assert.Equal(firstMeasurement.ReceivedAtUtc, first.GetProperty("receivedAtUtc").GetDateTime());
        Assert.Equal(25.0m, first.GetProperty("temperatureCelsius").GetDecimal());
        Assert.Equal(50.0m, first.GetProperty("relativeHumidityPercent").GetDecimal());

        JsonElement second = result[1];

        Assert.Equal(secondMeasurement.Id.Value, second.GetProperty("id").GetGuid());
        Assert.Equal(2, second.GetProperty("sourceMessageId").GetInt64());
        Assert.Equal(26.0m, second.GetProperty("temperatureCelsius").GetDecimal());
        Assert.Equal(51.0m, second.GetProperty("relativeHumidityPercent").GetDecimal());

        await _sensorReader.Received(1).GetByIdAsync(
            sensor.Id,
            Arg.Any<CancellationToken>()
        );

        await _measurementReader.Received(1).GetBySensorIdAsync(
            sensor.Id,
            fromUtc,
            toUtc,
            100,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSensorClimateMeasurements_ReturnsOkWithEmptyArray_WhenNoMeasurementsExist()
    {
        // Arrange
        Sensor sensor = CreateSensor();

        DateTime fromUtc = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

        _sensorReader.GetByIdAsync(
            sensor.Id,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(sensor));

        _measurementReader.GetBySensorIdAsync(
            sensor.Id,
            fromUtc,
            toUtc,
            100,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<IReadOnlyList<ClimateMeasurement>>(
            Array.Empty<ClimateMeasurement>()
        ));

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            BuildUrl(sensor.Id.Value, fromUtc, toUtc)
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument json = JsonDocument.Parse(content);

        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        Assert.Empty(json.RootElement.EnumerateArray());

        await _sensorReader.Received(1).GetByIdAsync(
            sensor.Id,
            Arg.Any<CancellationToken>()
        );

        await _measurementReader.Received(1).GetBySensorIdAsync(
            sensor.Id,
            fromUtc,
            toUtc,
            100,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSensorClimateMeasurements_ReturnsNotFound_WhenSensorDoesNotExist()
    {
        // Arrange
        SensorId sensorId = SensorId.New();

        DateTime fromUtc = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

        _sensorReader.GetByIdAsync(
            sensorId,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(null));

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            BuildUrl(sensorId.Value, fromUtc, toUtc)
        );

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await _sensorReader.Received(1).GetByIdAsync(
            sensorId,
            Arg.Any<CancellationToken>()
        );

        await _measurementReader.DidNotReceive().GetBySensorIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSensorClimateMeasurements_ReturnsBadRequest_WhenSensorIdIsInvalid()
    {
        // Arrange
        string invalidSensorId = "invalid-guid";

        DateTime fromUtc = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            BuildUrl(invalidSensorId, fromUtc, toUtc)
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

        Assert.Equal(
            "The sensor ID must be a valid GUID.",
            Assert.Single(sensorIdErrors.EnumerateArray()).GetString()
        );

        await _sensorReader.DidNotReceive().GetByIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<CancellationToken>()
        );

        await _measurementReader.DidNotReceive().GetBySensorIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSensorClimateMeasurements_ReturnsBadRequest_WhenTimeRangeIsInvalid()
    {
        // Arrange
        Sensor sensor = CreateSensor();

        DateTime fromUtc = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            BuildUrl(sensor.Id.Value, fromUtc, toUtc)
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
        JsonElement fromUtcErrors = errors.GetProperty("fromUtc");

        Assert.Equal(
            "The start time must not be later than the end time.",
            Assert.Single(fromUtcErrors.EnumerateArray()).GetString()
        );

        await _sensorReader.DidNotReceive().GetByIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<CancellationToken>()
        );

        await _measurementReader.DidNotReceive().GetBySensorIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    public async Task GetSensorClimateMeasurements_ReturnsBadRequest_WhenLimitIsOutOfRange(int limit)
    {
        // Arrange
        Sensor sensor = CreateSensor();

        DateTime fromUtc = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            $"{BuildUrl(sensor.Id.Value, fromUtc, toUtc)}&limit={limit}"
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
        JsonElement limitErrors = errors.GetProperty("limit");

        Assert.Equal(
            "The limit must be between 1 and 500.",
            Assert.Single(limitErrors.EnumerateArray()).GetString()
        );

        await _sensorReader.DidNotReceive().GetByIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<CancellationToken>()
        );

        await _measurementReader.DidNotReceive().GetBySensorIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSensorClimateMeasurements_ReturnsInternalServerError_WhenMeasurementReaderThrows()
    {
        // Arrange
        Sensor sensor = CreateSensor();

        DateTime fromUtc = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

        _sensorReader.GetByIdAsync(
            sensor.Id,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(sensor));

        _measurementReader.GetBySensorIdAsync(
            sensor.Id,
            fromUtc,
            toUtc,
            100,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromException<IReadOnlyList<ClimateMeasurement>>(
            new InvalidOperationException("Simulated failure.")
        ));

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            BuildUrl(sensor.Id.Value, fromUtc, toUtc)
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
            sensor.Id,
            Arg.Any<CancellationToken>()
        );

        await _measurementReader.Received(1).GetBySensorIdAsync(
            sensor.Id,
            fromUtc,
            toUtc,
            100,
            Arg.Any<CancellationToken>()
        );
    }

    private static Sensor CreateSensor()
    {
        return new Sensor(
            DeviceId.New(),
            SensorCategory.Climate,
            "Sensor Name",
            "Sensor Model",
            "SN12345"
        );

    }

    private static ClimateMeasurement CreateMeasurement(
        SourceMessageId? sourceMessageId = null,
        SensorId? sensorId = null,
        DateTime? measuredAtUtc = null,
        DateTime? receivedAtUtc = null,
        decimal? temperatureCelsius = null,
        decimal? relativeHumidityPercent = null)
    {
        return new ClimateMeasurement(
            sourceMessageId ?? new SourceMessageId(42),
            sensorId ?? SensorId.New(),
            measuredAtUtc ?? new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc),
            receivedAtUtc ?? new DateTime(2026, 6, 6, 12, 0, 1, DateTimeKind.Utc),
            temperatureCelsius,
            relativeHumidityPercent
        );
    }

    private static string BuildUrl(
        Guid sensorId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return BuildUrl(sensorId.ToString(), fromUtc, toUtc);
    }

    private static string BuildUrl(
        string sensorId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        string from = Uri.EscapeDataString(
            new DateTimeOffset(fromUtc).ToString("O")
        );

        string to = Uri.EscapeDataString(
            new DateTimeOffset(toUtc).ToString("O")
        );

        return $"/api/sensors/{sensorId}/measurements/climate" +
               $"?fromUtc={from}&toUtc={to}";
    }

    public void Dispose()
    {
        _client.Dispose();
        _testFactory.Dispose();
    }
}
