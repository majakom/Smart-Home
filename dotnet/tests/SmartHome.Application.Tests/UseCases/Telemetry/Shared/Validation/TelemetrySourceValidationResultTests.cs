using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Tests.UseCases.Telemetry.Shared.Validation;

public class TelemetrySourceValidationResultTests
{
    [Fact]
    public void ValidatedSensor_ReturnsSensor_WhenValidationSucceeded()
    {
        // Arrange
        Sensor sensor = CreateSensor();
        TelemetrySourceValidationResult result = new(null, sensor);

        // Act
        Sensor validatedSensor = result.ValidatedSensor;

        // Assert
        Assert.Same(sensor, validatedSensor);
    }

    [Fact]
    public void ValidatedSensor_ThrowsInvalidOperationException_WhenValidationWasRejected()
    {
        // Arrange
        Sensor sensor = CreateSensor();
        TelemetrySourceValidationResult result = new(TelemetryRejectionReason.SensorNotFound, sensor);

        // Act
        Action act = () => _ = result.ValidatedSensor;

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void ValidatedSensor_ThrowsInvalidOperationException_WhenSuccessfulResultHasNoSensor()
    {
        // Arrange
        TelemetrySourceValidationResult result = new(null);

        // Act
        Action act = () => _ = result.ValidatedSensor;

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    private static Sensor CreateSensor()
    {
        return new Sensor(
            DeviceId.New(),
            SensorCategory.Climate,
            "Sensor Name",
            "Sensor Model"
        );
    }
}
