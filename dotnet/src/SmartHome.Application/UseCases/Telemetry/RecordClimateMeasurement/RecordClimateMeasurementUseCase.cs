using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;

public sealed class RecordClimateMeasurementUseCase
{
    private readonly TelemetrySourceValidator _telemetrySourceValidator;
    private readonly IClimateMeasurementWriter _measurementWriter;
    private readonly TimeProvider _timeProvider;

    public RecordClimateMeasurementUseCase(
        TelemetrySourceValidator telemetrySourceValidator,
        IClimateMeasurementWriter measurementWriter,
        TimeProvider timeProvider
    )
    {
        _telemetrySourceValidator = telemetrySourceValidator;
        _measurementWriter = measurementWriter;
        _timeProvider = timeProvider;
    }

    public async Task<RecordClimateMeasurementResult> ExecuteAsync(
        RecordClimateMeasurementRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Validate Telemetry Source (Device + Sensor)
        TelemetrySourceValidationResult validation = await _telemetrySourceValidator.ValidateAsync(
            request.SensorId,
            request.DeviceId,
            SensorCategory.Climate,
            cancellationToken
        );

        if (validation.RejectionReason is TelemetryRejectionReason rejectionReason)
            return RecordClimateMeasurementResult.Rejected(rejectionReason);

        // 2. Create domain entity
        ClimateMeasurement measurement = new ClimateMeasurement(
            request.SourceMessageId,
            request.SensorId,
            request.MeasuredAtUtc,
            _timeProvider.GetUtcNow().UtcDateTime,
            request.TemperatureCelsius,
            request.RelativeHumidityPercent
        );

        // 3. Add measurement to db
        bool added = await _measurementWriter.TryAddAsync(
            measurement,
            cancellationToken
        );

        // 4. Return result
        if (!added)
            return RecordClimateMeasurementResult.Duplicate();

        return RecordClimateMeasurementResult.Recorded(measurement.Id);
    }
}
