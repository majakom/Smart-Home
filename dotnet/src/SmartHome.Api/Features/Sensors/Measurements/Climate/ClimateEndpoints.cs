namespace SmartHome.Api.Features.Sensors.Measurements.Climate;

public static class ClimateEndpoints
{
    public static IEndpointRouteBuilder MapClimateEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        var group = endpoints
            .MapGroup("/{sensorId}/measurements/climate")
            .WithTags("Sensor climate measurements");

        group.MapGet("/", GetSensorClimateMeasurements.GetSensorClimateMeasurementsEndpoint.Handle)
            .WithName("GetSensorClimateMeasurements")
            .WithSummary("Gets climate measurements for a sensor within a time range")
            .Produces<IReadOnlyList<ClimateMeasurementResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}
