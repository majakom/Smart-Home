namespace SmartHome.Application.UseCases.Telemetry.Shared.Results;

public enum TelemetryQueryFailureReason
{
    InvalidSensorId,
    SensorNotFound,
    InvalidSensorCategory,
    InvalidFromUtc,
    InvalidToUtc,
    InvalidTimeRange,
    InvalidLimit
}
