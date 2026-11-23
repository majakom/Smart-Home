using SmartHome.Domain.Sensors;

namespace SmartHome.Api.Features.Sensors;

public static class SensorResponseMapper
{
    public static SensorResponse Map(Sensor sensor)
    {
        return new SensorResponse(
            sensor.Id.Value,
            sensor.DeviceId.Value,
            sensor.Category.ToString(),
            sensor.Name,
            sensor.Model,
            sensor.SerialNumber?.Value,
            sensor.RegisteredAtUtc,
            sensor.IsEnabled
        );
    }
}
