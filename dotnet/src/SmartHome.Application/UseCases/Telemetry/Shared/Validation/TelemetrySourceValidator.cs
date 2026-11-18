using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Telemetry.Shared.Validation;

public sealed class TelemetrySourceValidator
{
    private readonly ISensorReader _sensorReader;
    private readonly IDeviceReader _deviceReader;

    public TelemetrySourceValidator(
        ISensorReader sensorReader,
        IDeviceReader deviceReader
    )
    {
        _sensorReader = sensorReader;
        _deviceReader = deviceReader;
    }

    public async Task<TelemetrySourceValidationResult> ValidateAsync(
        SensorId sensorId,
        DeviceId deviceId,
        SensorCategory expectedCategory,
        CancellationToken cancellationToken
    )
    {
        // 1. Retrieve sensor
        Sensor? sensor = await _sensorReader.GetByIdAsync(
            sensorId,
            cancellationToken
        );

        if (sensor is null)
            return new TelemetrySourceValidationResult(TelemetryRejectionReason.SensorNotFound);

        // 2. Validate sensorId matching deviceId
        if (sensor.DeviceId != deviceId)
            return new TelemetrySourceValidationResult(TelemetryRejectionReason.SensorDoesNotBelongToDevice);

        // 3. Retrieve deviceId
        Device? device = await _deviceReader.GetByIdAsync(
            sensor.DeviceId,
            cancellationToken
        );

        if (device is null)
            return new TelemetrySourceValidationResult(TelemetryRejectionReason.DeviceNotFound);

        // 4. Validate device state
        if (!device.IsEnabled)
            return new TelemetrySourceValidationResult(TelemetryRejectionReason.DeviceDisabled);

        // 5. Validate sensor state
        if (!sensor.IsEnabled)
            return new TelemetrySourceValidationResult(TelemetryRejectionReason.SensorDisabled);

        // 6. Validate category matching
        if (sensor.Category != expectedCategory)
            return new TelemetrySourceValidationResult(TelemetryRejectionReason.SensorCategoryMismatch);

        // Validation passed
        return new TelemetrySourceValidationResult(null, sensor);
    }
}
