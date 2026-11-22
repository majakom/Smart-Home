using SmartHome.Application.UseCases.Devices.GetDeviceById;
using SmartHome.Domain.Devices;

namespace SmartHome.Api.Features.Devices.GetDeviceById;

public static class GetDeviceByIdEndpoint
{
    public static async Task<IResult> Handle(
        string deviceId,
        GetDeviceByIdUseCase useCase,
        CancellationToken cancellationToken
    )
    {
        if (!Guid.TryParse(deviceId, out Guid parsedDeviceId))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["deviceId"] = ["The device ID must be a valid GUID."]
                }
            );
        }

        Device? device = await useCase.ExecuteAsync(
            new DeviceId(parsedDeviceId),
            cancellationToken
        );

        if (device is null)
            return Results.NotFound();

        DeviceResponse response = DeviceResponseMapper.Map(device);

        return Results.Ok(response);
    }
}
