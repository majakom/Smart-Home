using Microsoft.EntityFrameworkCore;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Domain.Devices;

namespace SmartHome.Infrastructure.Persistence.Adapters.Devices;

public sealed class EfDeviceReader : IDeviceReader
{
    private readonly SmartHomeDbContext _dbContext;

    public EfDeviceReader(SmartHomeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Device?> GetByIdAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext.Devices
            .AsNoTracking()
            .SingleOrDefaultAsync(
                device => device.Id == deviceId,
                cancellationToken
            );
    }
}
