using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Telemetry.Shared.Validation;

public sealed class TelemetryRequestValidator
{
    public TelemetryRejectionReason? Validate(
        SourceMessageId sourceMessageId,
        DeviceId deviceId,
        SensorId sensorId,
        DateTime measuredAtUtc
    )
    {
        if (sourceMessageId.Value <= 0)
            return TelemetryRejectionReason.InvalidSourceMessageId;

        if (deviceId.IsEmpty)
            return TelemetryRejectionReason.InvalidDeviceId;

        if (sensorId.IsEmpty)
            return TelemetryRejectionReason.InvalidSensorId;

        if (measuredAtUtc == default ||
            measuredAtUtc.Kind != DateTimeKind.Utc)
        {
            return TelemetryRejectionReason.InvalidMeasuredAtUtc;
        }

        return null;
    }
}