using SmartHome.Application.UseCases.Telemetry.Shared;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Domain.Measurements;

namespace SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;

public sealed record RecordClimateMeasurementResult
{
    public TelemetryOutcome Outcome { get; }
    public TelemetryRejectionReason? RejectionReason { get; }
    public ClimateMeasurementId? MeasurementId { get; }

    private RecordClimateMeasurementResult(
        TelemetryOutcome outcome,
        TelemetryRejectionReason? rejectionReason,
        ClimateMeasurementId? measurementId)
    {
        Outcome = outcome;
        RejectionReason = rejectionReason;
        MeasurementId = measurementId;
    }

    public static RecordClimateMeasurementResult Recorded(
        ClimateMeasurementId measurementId) =>
        new(TelemetryOutcome.Recorded, null, measurementId);

    public static RecordClimateMeasurementResult Duplicate() =>
        new(TelemetryOutcome.Duplicate, null, null);

    public static RecordClimateMeasurementResult Rejected(
        TelemetryRejectionReason reason) =>
        new(TelemetryOutcome.Rejected, reason, null);
}