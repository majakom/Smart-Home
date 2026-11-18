using Microsoft.EntityFrameworkCore;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Domain.Devices;

namespace SmartHome.Infrastructure.Persistence.Adapters.Devices;

public class EfDevicePresenceWriter : IDevicePresenceWriter
{
    private readonly SmartHomeDbContext _dbContext;

    public EfDevicePresenceWriter(SmartHomeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task MarkAsSeenAsync(
        DeviceId deviceId,
        DateTime receivedAtUtc,
        CancellationToken cancellationToken
    )
    {
        Device device = await _dbContext.Devices
            .SingleOrDefaultAsync(
                device => device.Id == deviceId,
                cancellationToken
            ) ?? throw new InvalidOperationException(
                $"Cannot mark device '{deviceId}' as seen because it was not found."
            );

        device.MarkAsSeen(receivedAtUtc);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
