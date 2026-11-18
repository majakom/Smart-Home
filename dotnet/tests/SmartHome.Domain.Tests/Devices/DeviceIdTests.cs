using SmartHome.Domain.Devices;

namespace SmartHome.Domain.Tests.Devices;

public class DeviceIdTests
{
    [Fact]
    public void Constructor_PreservesProvidedValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        DeviceId deviceId = new(value);

        // Assert
        Assert.Equal(value, deviceId.Value);
    }

    [Fact]
    public void New_ReturnsNonEmptyId()
    {
        // Act
        DeviceId deviceId = DeviceId.New();

        // Assert
        Assert.False(deviceId.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsTrue_WhenValueIsEmpty()
    {
        // Arrange
        DeviceId deviceId = new(Guid.Empty);

        // Act & Assert
        Assert.True(deviceId.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalse_WhenValueIsNotEmpty()
    {
        // Arrange
        DeviceId deviceId = new(Guid.NewGuid());

        // Act & Assert
        Assert.False(deviceId.IsEmpty);
    }

    [Fact]
    public void Default_ReturnsEmptyId()
    {
        // Act
        DeviceId deviceId = default;

        // Assert
        Assert.True(deviceId.IsEmpty);
    }

    [Fact]
    public void Equal_ReturnsTrue_WhenValuesMatch()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act & Assert
        Assert.Equal(new DeviceId(value), new DeviceId(value));
    }
}
