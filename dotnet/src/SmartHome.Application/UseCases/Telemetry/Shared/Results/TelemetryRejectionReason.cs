namespace SmartHome.Application.UseCases.Telemetry.Shared.Results;

public enum TelemetryRejectionReason
{
    SensorNotFound,
    SensorDoesNotBelongToDevice,
    DeviceNotFound,
    DeviceDisabled,
    SensorDisabled,
    SensorCategoryMismatch,
    InvalidSourceMessageId,
    InvalidDeviceId,
    InvalidSensorId,
    InvalidMeasuredAtUtc,
    InvalidMeasurementData
}
