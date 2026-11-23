using NSubstitute;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.UseCases.Sensors.GetSensors;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Tests.UseCases.Sensors.GetSensors;

public class GetSensorsUseCaseTests
{
    private readonly ISensorReader _sensorReader;
    private readonly GetSensorsUseCase _useCase;

    public GetSensorsUseCaseTests()
    {
        _sensorReader = Substitute.For<ISensorReader>();
        _useCase = new GetSensorsUseCase(_sensorReader);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSensors_WhenSensorsExist()
    {
        // Arrange
        Sensor firstSensor = new Sensor(DeviceId.New(), SensorCategory.Climate, "Sensor Name A", "Sensor Model A");
        Sensor secondSensor = new Sensor(DeviceId.New(), SensorCategory.Climate, "Sensor Name B", "Sensor Model B");

        IReadOnlyList<Sensor> expectedSensors = [firstSensor, secondSensor];

        _sensorReader.GetAllAsync(
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(expectedSensors));

        // Act
        IReadOnlyList<Sensor> sensors = await _useCase.ExecuteAsync();

        // Assert
        Assert.Equal(2, sensors.Count);
        Assert.Collection(
            sensors,
            sensor => Assert.Same(firstSensor, sensor),
            sensor => Assert.Same(secondSensor, sensor)
        );

        await _sensorReader.Received(1).GetAllAsync(
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyCollection_WhenNoSensorsExist()
    {
        // Arrange
        IReadOnlyList<Sensor> expectedSensors = [];

        _sensorReader.GetAllAsync(
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(expectedSensors));

        // Act
        IReadOnlyList<Sensor> sensors = await _useCase.ExecuteAsync();

        // Assert
        Assert.Empty(sensors);

        await _sensorReader.Received(1).GetAllAsync(
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToReader()
    {
        // Arrange
        CancellationToken cancellationToken = new();

        _sensorReader.GetAllAsync(
            cancellationToken
        ).Returns(Task.FromResult<IReadOnlyList<Sensor>>([]));

        // Act
        await _useCase.ExecuteAsync(cancellationToken);

        // Assert
        await _sensorReader.Received(1).GetAllAsync(
            cancellationToken
        );
    }
}