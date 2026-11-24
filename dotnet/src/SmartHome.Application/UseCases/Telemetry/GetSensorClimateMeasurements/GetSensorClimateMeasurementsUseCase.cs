using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Telemetry.GetSensorClimateMeasurements;

public sealed class GetSensorClimateMeasurementsUseCase
{
    private readonly IClimateMeasurementReader _measurementReader;
    private readonly TelemetryQueryValidator _queryValidator;
    private readonly TelemetryQuerySensorValidator _sensorValidator;

    public GetSensorClimateMeasurementsUseCase(
        IClimateMeasurementReader measurementReader,
        TelemetryQueryValidator queryValidator,
        TelemetryQuerySensorValidator sensorValidator
    )
    {
        _measurementReader = measurementReader;
        _queryValidator = queryValidator;
        _sensorValidator = sensorValidator;
    }

    public async Task<GetSensorClimateMeasurementsResult> ExecuteAsync(
        GetSensorClimateMeasurementsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Validate Request
        TelemetryQueryFailureReason? queryFailure =
            _queryValidator.Validate(request.FromUtc, request.ToUtc, request.Limit);

        if (queryFailure is TelemetryQueryFailureReason queryReason)
            return GetSensorClimateMeasurementsResult.Failure(queryReason);
        
        // 2. Validate Sensor
        TelemetryQueryFailureReason? sensorFailure = await _sensorValidator.ValidateAsync(
            request.SensorId,
            SensorCategory.Climate,
            cancellationToken
        );

        if (sensorFailure is TelemetryQueryFailureReason sensorReason)
            return GetSensorClimateMeasurementsResult.Failure(sensorReason);

        // 3. Retrieve Measurements
        IReadOnlyList<ClimateMeasurement> measurements = await _measurementReader.GetBySensorIdAsync(
            request.SensorId,
            request.FromUtc,
            request.ToUtc,
            request.Limit,
            cancellationToken
        );

        // 4. Return Result
        return GetSensorClimateMeasurementsResult.Success(measurements);
    }
}
