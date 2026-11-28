using NSubstitute;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.UseCases.Devices.GetDevices;
using SmartHome.Domain.Devices;

namespace SmartHome.Application.Tests.UseCases.Devices.GetDevices;

public class GetDevicesUseCaseTests
{
    private readonly IDeviceReader _deviceReader;
    private readonly GetDevicesUseCase _useCase;

    public GetDevicesUseCaseTests()
    {
        _deviceReader = Substitute.For<IDeviceReader>();
        _useCase = new GetDevicesUseCase(_deviceReader);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDevices_WhenDevicesExist()
    {
        // Arrange
        Device firstDevice = new Device("Device Name A","Device Model A");
        Device secondDevice = new Device("Device Name B","Device Model B");

        IReadOnlyList<Device> expectedDevices = [firstDevice, secondDevice];

        _deviceReader.GetAllAsync(
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(expectedDevices));

        // Act
        IReadOnlyList<Device> devices = await _useCase.ExecuteAsync();

        // Assert
        Assert.Equal(2, devices.Count);
        Assert.Collection(
            devices,
            device => Assert.Same(firstDevice, device),
            device => Assert.Same(secondDevice, device)
        );

        await _deviceReader.Received(1).GetAllAsync(
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyCollection_WhenNoDevicesExist()
    {
        // Arrange
        IReadOnlyList<Device> expectedDevices = [];

        _deviceReader.GetAllAsync(
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(expectedDevices));

        // Act
        IReadOnlyList<Device> devices = await _useCase.ExecuteAsync();

        // Assert
        Assert.Empty(devices);

        await _deviceReader.Received(1).GetAllAsync(
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToReader()
    {
        // Arrange
        CancellationToken cancellationToken = new();

        _deviceReader.GetAllAsync(
            cancellationToken
        ).Returns(Task.FromResult<IReadOnlyList<Device>>([]));

        // Act
        await _useCase.ExecuteAsync(cancellationToken);

        // Assert
        await _deviceReader.Received(1).GetAllAsync(
            cancellationToken
        );
    }
}