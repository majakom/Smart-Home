using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Telemetry.Shared.Validation;

public sealed record TelemetrySourceValidationResult(
    TelemetryRejectionReason? RejectionReason,
    Sensor? Sensor = null
)
{
    public Sensor ValidatedSensor =>
        RejectionReason is null
            ? Sensor ?? throw new InvalidOperationException(
                "A successful telemetry validation must include its sensor.")
            : throw new InvalidOperationException(
                "A rejected telemetry source has no validated sensor.");
}
