using SmartHome.Application.UseCases.Telemetry.Shared.Results;

namespace SmartHome.Application.UseCases.Telemetry.Shared.Validation;

public sealed class TelemetryQueryValidator
{
    public TelemetryQueryFailureReason? Validate(
        DateTime fromUtc,
        DateTime toUtc
    )
    {
        if (fromUtc == default || fromUtc.Kind != DateTimeKind.Utc)
            return TelemetryQueryFailureReason.InvalidFromUtc;

        if (toUtc == default || toUtc.Kind != DateTimeKind.Utc)
            return TelemetryQueryFailureReason.InvalidToUtc;
        
        if (fromUtc > toUtc)
            return TelemetryQueryFailureReason.InvalidTimeRange;

        return null;
    }
}
