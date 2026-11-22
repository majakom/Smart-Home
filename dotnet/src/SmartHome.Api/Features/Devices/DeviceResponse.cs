namespace SmartHome.Api.Features.Devices;

public sealed record DeviceResponse(
    Guid Id,
    string Name,
    string Model,
    DateTime RegisteredAtUtc,
    DateTime? LastSeenAtUtc,
    bool IsEnabled
);
