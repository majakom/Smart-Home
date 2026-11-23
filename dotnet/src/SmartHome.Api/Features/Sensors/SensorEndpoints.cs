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
        
        group.MapGet("/{sensorId}", GetSensorById.GetSensorByIdEndpoint.Handle)
            .WithName("GetSensorById")
            .WithSummary("Gets a sensor by its ID")
            .Produces<SensorResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
