using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Domain.Measurements;

namespace SmartHome.Infrastructure.Persistence.Adapters.Telemetry;

public sealed class EfClimateMeasurementWriter : IClimateMeasurementWriter
{
    private readonly SmartHomeDbContext _dbContext;

    public EfClimateMeasurementWriter(SmartHomeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> TryAddAsync(
        ClimateMeasurement measurement,
        CancellationToken cancellationToken
    )
    {
        _dbContext.ClimateMeasurements.Add(measurement);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception)
            when (IsDuplicateMessage(exception))
        {
            _dbContext.Entry(measurement).State = EntityState.Detached;
            return false;
        }
    }
    private static bool IsDuplicateMessage(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: IndexNames.ClimateMeasurementSensorSourceMessage
        };
    }
}
