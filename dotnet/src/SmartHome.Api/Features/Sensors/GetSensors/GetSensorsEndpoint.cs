using SmartHome.Application.UseCases.Sensors.GetSensors;
using SmartHome.Domain.Sensors;

namespace SmartHome.Api.Features.Sensors.GetSensors;

public static class GetSensorsEndpoint
{
    public static async Task<IResult> Handle(
        GetSensorsUseCase useCase,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<Sensor> sensors = await useCase.ExecuteAsync(cancellationToken);

        IReadOnlyList<SensorResponse> response = sensors
            .Select(SensorResponseMapper.Map)
            .ToList();

        return Results.Ok(response);
    }
}
