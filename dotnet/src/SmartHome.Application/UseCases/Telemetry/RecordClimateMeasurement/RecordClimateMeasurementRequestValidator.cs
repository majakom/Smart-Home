using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;

namespace SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;

public sealed class RecordClimateMeasurementRequestValidator
{
    private readonly TelemetryRequestValidator _telemetryRequestValidator;

    public RecordClimateMeasurementRequestValidator(
        TelemetryRequestValidator telemetryRequestValidator
    )
    {
        _telemetryRequestValidator = telemetryRequestValidator;
    }

    public TelemetryRejectionReason? Validate(
        RecordClimateMeasurementRequest request
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        TelemetryRejectionReason? commonRejection =
            _telemetryRequestValidator.Validate(
                request.SourceMessageId,
                request.DeviceId,
                request.SensorId,
                request.MeasuredAtUtc
            );

        if (commonRejection is not null)
            return commonRejection;

        if (request.RelativeHumidityPercent is < 0m or > 100m)
            return TelemetryRejectionReason.InvalidMeasurementData;

        return null;
    }
}