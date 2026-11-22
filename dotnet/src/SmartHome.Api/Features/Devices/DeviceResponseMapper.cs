using SmartHome.Domain.Devices;

namespace SmartHome.Api.Features.Devices;

public static class DeviceResponseMapper
{
    public static DeviceResponse Map(Device device)
    {
        return new DeviceResponse(
            device.Id.Value,
            device.Name,
            device.Model,
            device.RegisteredAtUtc,
            device.LastSeenAtUtc,
            device.IsEnabled
        );
    }
}
