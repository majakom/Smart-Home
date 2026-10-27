using SmartHome.Domain.Devices;
using SerialNumberValue = SmartHome.Domain.Shared.SerialNumber;

namespace SmartHome.Domain.Sensors;

public sealed class Sensor
{
    public SensorId Id { get; private set; }
    public DeviceId DeviceId { get; private set; }
    public SensorCategory Category { get; private set; }
    public string Name { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public SerialNumberValue? SerialNumber { get; private set; }
    public DateTime RegisteredAtUtc { get; private set; }
    public bool IsEnabled { get; private set; }

    private Sensor() { }

    public Sensor(
        DeviceId deviceId,
        SensorCategory category,
        string name,
        string model,
        string? serialNumber = null)
    {
        if (deviceId.IsEmpty)
            throw new ArgumentException("Device ID cannot be empty.", nameof(deviceId));

        if (!Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(nameof(category), "Invalid sensor type.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Sensor name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Sensor model cannot be empty", nameof(model));

        Id = SensorId.New();
        DeviceId = deviceId;
        Category = category;
        Name = name.Trim();
        Model = model.Trim();
        SerialNumber = SerialNumberValue.Parse(serialNumber);
        RegisteredAtUtc = DateTime.UtcNow;
        IsEnabled = true;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Sensor name cannot be empty.", nameof(name));
        
        Name = name.Trim();
    }

    public void ChangeModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Sensor model cannot be empty.", nameof(model));
        
        Model = model.Trim();
    }

    public void ChangeSerialNumber(string? serialNumber)
    {
        SerialNumber = SerialNumberValue.Parse(serialNumber);
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;
}
