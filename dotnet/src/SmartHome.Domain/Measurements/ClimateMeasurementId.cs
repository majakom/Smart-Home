namespace SmartHome.Domain.Measurements;

public readonly record struct ClimateMeasurementId(Guid Value)
{
    public static ClimateMeasurementId New() => new(Guid.NewGuid());

    public bool IsEmpty => Value == Guid.Empty;
}
