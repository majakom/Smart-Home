using SmartHome.Application.UseCases.Telemetry.GetSensorClimateMeasurements;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Domain.Sensors;

namespace SmartHome.Api.Features.Sensors.Measurements.Climate.GetSensorClimateMeasurements;

public static class GetSensorClimateMeasurementsEndpoint
{
    public static async Task<IResult> Handle(
        string sensorId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        GetSensorClimateMeasurementsUseCase getSensorClimateMeasurementsUseCase,
        CancellationToken cancellationToken,
        int limit = 100
    )
    {
        if (!Guid.TryParse(sensorId, out Guid parsedSensorId))
        {
            return ValidationError(
                "sensorId",
                "The sensor ID must be a valid GUID."
            );
        }

        GetSensorClimateMeasurementsRequest request = new(
            new SensorId(parsedSensorId),
            fromUtc.UtcDateTime,
            toUtc.UtcDateTime,
            limit
        );

        GetSensorClimateMeasurementsResult result = 
            await getSensorClimateMeasurementsUseCase.ExecuteAsync(
                request,
                cancellationToken
            );

        if (result.Outcome == TelemetryQueryOutcome.Failed)
        {
            if (result.FailureReason is not TelemetryQueryFailureReason reason)
                return Results.Problem("The measurement query failed.");

            return MapFailure(reason);
        }

        IReadOnlyList<ClimateMeasurementResponse> response = result.Measurements
            .Select(ClimateMeasurementResponseMapper.Map)
            .ToList();

        return Results.Ok(response);
    }

    private static IResult MapFailure(TelemetryQueryFailureReason reason)
    {
        return reason switch
        {
            TelemetryQueryFailureReason.SensorNotFound =>
                Results.NotFound(),

            TelemetryQueryFailureReason.InvalidSensorCategory =>
                ValidationError(
                    "sensorId",
                    "The sensor does not measure climate data."
                ),

            TelemetryQueryFailureReason.InvalidSensorId =>
                ValidationError(
                    "sensorId",
                    "The sensor ID is invalid."
                ),

            TelemetryQueryFailureReason.InvalidFromUtc =>
                ValidationError(
                    "fromUtc",
                    "The start time must be a valid UTC timestamp."
                ),

            TelemetryQueryFailureReason.InvalidToUtc =>
                ValidationError(
                    "toUtc",
                    "The end time must be a valid UTC timestamp."
                ),

            TelemetryQueryFailureReason.InvalidTimeRange =>
                ValidationError(
                    "fromUtc",
                    "The start time must not be later than the end time."
                ),

            TelemetryQueryFailureReason.InvalidLimit =>
                ValidationError(
                    "limit",
                    "The limit must be between 1 and 500."
                ),

            _ => Results.Problem(
                "An unexpected measurement query failure occurred."
            )
        };
    }

    private static IResult ValidationError(string key, string message)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                [key] = [message]
            }
        );
    }
}
