using SmartHome.Application.UseCases.Telemetry.Shared.Results;

namespace SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry;

public sealed class TelemetryOutcomeLogger(ILogger<TelemetryOutcomeLogger> logger)
{
    public void Log(
        TelemetryOutcome outcome,
        TelemetryRejectionReason? rejectionReason,
        string category,
        string sensorId
    )
    {
        switch (outcome)
        {
            case TelemetryOutcome.Recorded:
                logger.LogInformation(
                    "Recorded {Category} measurement from sensor {SensorId}.",
                    category,
                    sensorId);
                break;

            case TelemetryOutcome.Duplicate:
                logger.LogDebug(
                    "Ignored duplicate {Category} measurement from sensor {SensorId}.",
                    category,
                    sensorId);
                break;

            case TelemetryOutcome.Rejected:
                logger.LogWarning(
                    "Rejected {Category} measurement from sensor {SensorId}. Reason: {Reason}",
                    category,
                    sensorId,
                    rejectionReason);
                break;
        }
    }
}
