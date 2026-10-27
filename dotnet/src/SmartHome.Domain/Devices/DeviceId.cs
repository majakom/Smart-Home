namespace SmartHome.Domain.Devices;

public readonly record struct DeviceId(Guid Value)
{
    public static DeviceId New() => new(Guid.NewGuid());

    public bool IsEmpty => Value == Guid.Empty;
}
