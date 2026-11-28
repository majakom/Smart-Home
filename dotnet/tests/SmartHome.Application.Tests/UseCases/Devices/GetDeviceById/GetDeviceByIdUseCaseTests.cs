using NSubstitute;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.UseCases.Devices.GetDeviceById;
using SmartHome.Domain.Devices;

namespace SmartHome.Application.Tests.UseCases.Devices.GetDeviceById;

public class GetDeviceByIdUseCaseTests
{
    private readonly IDeviceReader _deviceReader;
    private readonly GetDeviceByIdUseCase _useCase;

    public GetDeviceByIdUseCaseTests()
    {
        _deviceReader = Substitute.For<IDeviceReader>();
        _useCase = new GetDeviceByIdUseCase(_deviceReader);
    }

     [Fact]
    public async Task ExecuteAsync_ReturnsDevice_WhenDeviceExists()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");

        _deviceReader.GetByIdAsync(
            device.Id,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Device?>(device));

        // Act
        Device? result = await _useCase.ExecuteAsync(device.Id);

        // Assert
        Assert.Same(device, result);

        await _deviceReader.Received(1).GetByIdAsync(
            device.Id,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenDeviceDoesNotExist()
    {
        // Arrange
        DeviceId deviceId = DeviceId.New();

        _deviceReader.GetByIdAsync(
            deviceId,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Device?>(null));

        // Act
        Device? result = await _useCase.ExecuteAsync(deviceId);

        // Assert
        Assert.Null(result);

        await _deviceReader.Received(1).GetByIdAsync(
            deviceId,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToReader()
    {
        // Arrange
        DeviceId deviceId = DeviceId.New();
        CancellationToken cancellationToken = new();
        
        _deviceReader.GetByIdAsync(
            deviceId,
            cancellationToken
        ).Returns(Task.FromResult<Device?>(null));

        // Act
        await _useCase.ExecuteAsync(deviceId, cancellationToken);

        // Assert
        await _deviceReader.Received(1).GetByIdAsync(
            deviceId,
            cancellationToken
        );
    }
}
