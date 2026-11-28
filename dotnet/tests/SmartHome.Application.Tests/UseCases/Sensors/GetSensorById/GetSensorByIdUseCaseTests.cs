using NSubstitute;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.UseCases.Sensors.GetSensorById;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Tests.UseCases.Sensors.GetSensorById;

public class GetSensorByIdUseCaseTests
{
    private readonly ISensorReader _sensorReader;
    private readonly GetSensorByIdUseCase _useCase;

    public GetSensorByIdUseCaseTests()
    {
        _sensorReader = Substitute.For<ISensorReader>();
        _useCase = new GetSensorByIdUseCase(_sensorReader);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSensor_WhenSensorExists()
    {
        // Arrange
        Sensor sensor = new Sensor(DeviceId.New(), SensorCategory.Climate, "Test Sensor", "Model X");

        _sensorReader.GetByIdAsync(
            sensor.Id,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(sensor));

        // Act
        Sensor? result = await _useCase.ExecuteAsync(sensor.Id);

        // Assert
        Assert.Same(sensor, result);
        
        await _sensorReader.Received(1).GetByIdAsync(
            sensor.Id,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenSensorDoesNotExist()
    {
        // Arrange
        SensorId sensorId = SensorId.New();

        _sensorReader.GetByIdAsync(
            sensorId,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(null));

        // Act
        Sensor? result = await _useCase.ExecuteAsync(sensorId);

        // Assert
        Assert.Null(result);

        await _sensorReader.Received(1).GetByIdAsync(
            sensorId,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToReader()
    {
        // Arrange
        SensorId sensorId = SensorId.New();
        CancellationToken cancellationToken = new();

        _sensorReader.GetByIdAsync(
            sensorId,
            cancellationToken
        ).Returns(Task.FromResult<Sensor?>(null));

        // Act
        await _useCase.ExecuteAsync(sensorId, cancellationToken);

        // Assert
        await _sensorReader.Received(1).GetByIdAsync(
            sensorId,
            cancellationToken
        );
    }
}