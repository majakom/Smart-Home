using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;

public sealed class RecordClimateMeasurementUseCase
{
    private readonly TelemetrySourceValidator _telemetrySourceValidator;
    private readonly RecordClimateMeasurementRequestValidator _requestValidator;
    private readonly IClimateMeasurementWriter _measurementWriter;
    private readonly TimeProvider _timeProvider;

    public RecordClimateMeasurementUseCase(
        TelemetrySourceValidator telemetrySourceValidator,
        RecordClimateMeasurementRequestValidator requestValidator,
        IClimateMeasurementWriter measurementWriter,
        TimeProvider timeProvider
    )
    {
        _telemetrySourceValidator = telemetrySourceValidator;
        _requestValidator = requestValidator;
        _measurementWriter = measurementWriter;
        _timeProvider = timeProvider;
    }

    public async Task<RecordClimateMeasurementResult> ExecuteAsync(
        RecordClimateMeasurementRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Validate Request
        TelemetryRejectionReason? requestRejection = _requestValidator.Validate(request);

        if (requestRejection is TelemetryRejectionReason reason)
            return RecordClimateMeasurementResult.Rejected(reason);

        // 2. Validate Telemetry Source (Device + Sensor)
        TelemetrySourceValidationResult validation = await _telemetrySourceValidator.ValidateAsync(
            request.SensorId,
            request.DeviceId,
            SensorCategory.Climate,
            cancellationToken
        );

        if (validation.RejectionReason is TelemetryRejectionReason rejectionReason)
            return RecordClimateMeasurementResult.Rejected(rejectionReason);

        // 3. Create domain entity
        ClimateMeasurement measurement = new ClimateMeasurement(
            request.SourceMessageId,
            request.SensorId,
            request.MeasuredAtUtc,
            _timeProvider.GetUtcNow().UtcDateTime,
            request.TemperatureCelsius,
            request.RelativeHumidityPercent
        );

        // 4. Add measurement to db
        bool added = await _measurementWriter.TryAddAsync(
            measurement,
            cancellationToken
        );

        // 5. Return result
        if (!added)
            return RecordClimateMeasurementResult.Duplicate();

        return RecordClimateMeasurementResult.Recorded(measurement.Id);
    }
}
