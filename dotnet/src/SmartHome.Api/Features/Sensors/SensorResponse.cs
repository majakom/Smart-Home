namespace SmartHome.Api.Features.Sensors;

public sealed record SensorResponse(
    Guid Id,
    Guid DeviceId,
    string Category,
    string Name,
    string Model,
    string? SerialNumber,
    DateTime RegisteredAtUtc,
    bool IsEnabled
);
