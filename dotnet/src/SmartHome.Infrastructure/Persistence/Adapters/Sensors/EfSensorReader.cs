using Microsoft.EntityFrameworkCore;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Domain.Sensors;

namespace SmartHome.Infrastructure.Persistence.Adapters.Sensors;

public sealed class EfSensorReader : ISensorReader
{
    private readonly SmartHomeDbContext _dbContext;

    public EfSensorReader(SmartHomeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Sensor?> GetByIdAsync(
        SensorId sensorId,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext.Sensors
            .AsNoTracking()
            .SingleOrDefaultAsync(
                sensor => sensor.Id == sensorId,
                cancellationToken
            );
    }
}
