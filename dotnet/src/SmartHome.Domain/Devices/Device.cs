namespace SmartHome.Domain.Devices;

public sealed class Device
{
    public DeviceId Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public DateTime RegisteredAtUtc { get; private set; }
    public DateTime? LastSeenAtUtc { get; private set; }
    public bool IsEnabled { get; private set; }

    private Device() { }

    public Device(
        string name,
        string model)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Device name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Device model cannot be empty.", nameof(model));
        
        Id = DeviceId.New();
        Name = name.Trim();
        Model = model.Trim();
        RegisteredAtUtc = DateTime.UtcNow;
        IsEnabled = true;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Device name cannot be empty.", nameof(name));
        
        Name = name.Trim();
    }

    public void ChangeModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Device model cannot be empty.", nameof(model));
        
        Model = model.Trim();
    }

    public void MarkAsSeen(DateTime timestampUtc)
    {
        if (timestampUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Timestamp must be in UTC.", nameof(timestampUtc));
        
        if (LastSeenAtUtc is null || timestampUtc > LastSeenAtUtc)
            LastSeenAtUtc = timestampUtc;
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;
}
