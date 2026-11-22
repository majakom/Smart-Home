using SmartHome.Application.Abstractions.Devices;
using SmartHome.Domain.Devices;

namespace SmartHome.Application.UseCases.Devices.GetDeviceById;

public sealed class GetDeviceByIdUseCase
{
    private readonly IDeviceReader _deviceReader;

    public GetDeviceByIdUseCase(IDeviceReader deviceReader)
    {
        _deviceReader = deviceReader;
    }

    public Task<Device?> ExecuteAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken = default
    )
    {
        return _deviceReader.GetByIdAsync(deviceId, cancellationToken);
    }
}
