using SmartHome.Application.UseCases.Sensors.GetSensorById;
using SmartHome.Domain.Sensors;

namespace SmartHome.Api.Features.Sensors.GetSensorById;

public static class GetSensorByIdEndpoint
{
    public static async Task<IResult> Handle(
        string sensorId,
        GetSensorByIdUseCase getSensorByIdUseCase,
        CancellationToken cancellationToken
    )
    {
        if (!Guid.TryParse(sensorId, out Guid parsedSensorId))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["sensorId"] = ["The sensor ID must be a valid GUID."]
                }
            );
        }

        Sensor? sensor = await getSensorByIdUseCase.ExecuteAsync(
            new SensorId(parsedSensorId),
            cancellationToken
        );

        if (sensor is null)
            return Results.NotFound();

        SensorResponse response = SensorResponseMapper.Map(sensor);

        return Results.Ok(response);
    }
}
