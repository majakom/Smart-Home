namespace SmartHome.Api.Features.Sensors;

public static class SensorEndpoints
{
    public static IEndpointRouteBuilder MapSensorEndpoints(
        this IEndpointRouteBuilder app
    )
    {
        var group = app.MapGroup("/api/sensors")
            .WithTags("Sensors");

        group.MapGet("", GetSensors.GetSensorsEndpoint.Handle)
            .WithName("GetSensors")
            .WithSummary("Get all registered sensors")
            .Produces<IReadOnlyList<SensorResponse>>(StatusCodes.Status200OK);
        
        return app;
    }
}
