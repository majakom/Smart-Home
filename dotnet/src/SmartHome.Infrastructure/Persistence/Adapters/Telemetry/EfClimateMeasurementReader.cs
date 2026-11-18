using Microsoft.EntityFrameworkCore;
using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Infrastructure.Persistence.Adapters.Telemetry;

public class EfClimateMeasurementReader : IClimateMeasurementReader
{
    private readonly SmartHomeDbContext _dbContext;

    public EfClimateMeasurementReader(SmartHomeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IReadOnlyList<ClimateMeasurement>> GetBySensorIdAsync(
        SensorId sensorId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext.ClimateMeasurements
            .AsNoTracking()
            .Where(measurement =>
                measurement.SensorId == sensorId &&
                measurement.MeasuredAtUtc >= fromUtc &&
                measurement.MeasuredAtUtc <= toUtc
            ).OrderBy(measurement => measurement.MeasuredAtUtc)
            .ToListAsync(cancellationToken);
    }
}
