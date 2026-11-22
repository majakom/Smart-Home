using SmartHome.Application.UseCases.Devices.GetDevices;
using SmartHome.Domain.Devices;

namespace SmartHome.Api.Features.Devices.GetDevices;

public static class GetDevicesEndpoint
{
    public static async Task<IResult> Handle(
        GetDevicesUseCase useCase,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<Device> devices = await useCase.ExecuteAsync(cancellationToken);

        IReadOnlyList<DeviceResponse> response = devices
            .Select(DeviceResponseMapper.Map)
            .ToList();

        return Results.Ok(response);
    }
}
