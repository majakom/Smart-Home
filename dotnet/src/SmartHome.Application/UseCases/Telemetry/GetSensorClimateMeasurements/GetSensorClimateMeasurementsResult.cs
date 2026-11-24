using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Domain.Measurements;

namespace SmartHome.Application.UseCases.Telemetry.GetSensorClimateMeasurements;

public sealed record GetSensorClimateMeasurementsResult
{
   public TelemetryQueryOutcome Outcome { get; }
   public TelemetryQueryFailureReason? FailureReason { get; }
   public IReadOnlyList<ClimateMeasurement> Measurements { get; }

   private GetSensorClimateMeasurementsResult(
       TelemetryQueryOutcome outcome,
       TelemetryQueryFailureReason? failureReason,
       IReadOnlyList<ClimateMeasurement> measurements)
   {
       Outcome = outcome;
       FailureReason = failureReason;
       Measurements = measurements;
   }

    public static GetSensorClimateMeasurementsResult Success(
       IReadOnlyList<ClimateMeasurement> measurements) =>
       new(TelemetryQueryOutcome.Succeeded, null, measurements);

    public static GetSensorClimateMeasurementsResult Failure(
        TelemetryQueryFailureReason reason) =>
        new(TelemetryQueryOutcome.Failed, reason, Array.Empty<ClimateMeasurement>());
}
