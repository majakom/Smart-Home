using SmartHome.Domain.Devices;

namespace SmartHome.Application.Abstractions.Devices;

public interface IDeviceReader
{
    Task<Device?> GetByIdAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken
    );
}
