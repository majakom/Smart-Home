namespace SmartHome.Domain.Sensors;

public readonly record struct SensorId(Guid Value)
{
    public static SensorId New() => new(Guid.NewGuid());

    public bool IsEmpty => Value == Guid.Empty;
}
