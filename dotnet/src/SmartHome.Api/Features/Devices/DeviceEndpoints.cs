namespace SmartHome.Api.Features.Devices;

public static class DeviceEndpoints
{
    public static IEndpointRouteBuilder MapDeviceEndpoints(
        this IEndpointRouteBuilder app
    )
    {
        var group = app.MapGroup("/api/devices")
            .WithTags("Devices");

        group.MapGet("", GetDevices.GetDevicesEndpoint.Handle)
            .WithName("GetDevices")
            .WithSummary("Gets all registered devices")
            .Produces<IReadOnlyList<DeviceResponse>>(StatusCodes.Status200OK);

        return app;
    }
}
