using SmartHome.Application.UseCases.Telemetry.Shared.Results;

namespace SmartHome.Application.UseCases.Telemetry.Shared.Validation;

public sealed class TelemetryQueryValidator
{
    private const int MaximumLimit = 500;

    public TelemetryQueryFailureReason? Validate(
        DateTime fromUtc,
        DateTime toUtc,
        int limit
    )
    {
        if (fromUtc == default || fromUtc.Kind != DateTimeKind.Utc)
            return TelemetryQueryFailureReason.InvalidFromUtc;

        if (toUtc == default || toUtc.Kind != DateTimeKind.Utc)
            return TelemetryQueryFailureReason.InvalidToUtc;
        
        if (fromUtc > toUtc)
            return TelemetryQueryFailureReason.InvalidTimeRange;

        if (limit is < 1 or > MaximumLimit)
            return TelemetryQueryFailureReason.InvalidLimit;

        return null;
    }
}
