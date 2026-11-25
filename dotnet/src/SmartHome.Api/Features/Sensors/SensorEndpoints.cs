using SmartHome.Api.Features.Sensors.Measurements.Climate;

namespace SmartHome.Api.Features.Sensors;

public static class SensorEndpoints
{
    public static IEndpointRouteBuilder MapSensorEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        var group = endpoints.MapGroup("/api/sensors")
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

        group.MapClimateEndpoints();

        return endpoints;
    }
}
