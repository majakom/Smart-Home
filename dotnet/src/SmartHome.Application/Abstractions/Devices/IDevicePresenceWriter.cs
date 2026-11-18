using SmartHome.Domain.Devices;
namespace SmartHome.Application.Abstractions.Devices;

public interface IDevicePresenceWriter
{
    Task MarkAsSeenAsync(
        DeviceId deviceId,
        DateTime receivedAtUtc,
        CancellationToken cancellationToken);
}
