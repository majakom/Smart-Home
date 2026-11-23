using SmartHome.Application.Abstractions.Devices;
using SmartHome.Domain.Devices;

namespace SmartHome.Application.UseCases.Devices.GetDevices;

public sealed class GetDevicesUseCase
{
    private readonly IDeviceReader _deviceReader;

    public GetDevicesUseCase(IDeviceReader deviceReader)
    {
        _deviceReader = deviceReader;
    }

    public Task<IReadOnlyList<Device>> ExecuteAsync(
        CancellationToken cancellationToken = default
    )
    {
        return _deviceReader.GetAllAsync(cancellationToken);
    }
}
