using SmartHome.Domain.Sensors;

namespace SmartHome.Domain.Tests.Sensors;

public class SensorIdTests
{
    [Fact]
    public void Constructor_PreservesProvidedValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        SensorId sensorId = new(value);

        // Assert
        Assert.Equal(value, sensorId.Value);
    }

    [Fact]
    public void New_ReturnsNonEmptyId()
    {
        // Act
        SensorId sensorId = SensorId.New();

        // Assert
        Assert.False(sensorId.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsTrue_WhenValueIsEmpty()
    {
        // Arrange
        SensorId sensorId = new(Guid.Empty);

        // Act & Assert
        Assert.True(sensorId.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalse_WhenValueIsNotEmpty()
    {
        // Arrange
        SensorId sensorId = new(Guid.NewGuid());

        // Act & Assert
        Assert.False(sensorId.IsEmpty);
    }

    [Fact]
    public void Default_ReturnsEmptyId()
    {
        // Act
        SensorId sensorId = default;

        // Assert
        Assert.True(sensorId.IsEmpty);
    }

    [Fact]
    public void Equal_ReturnsTrue_WhenValuesMatch()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act & Assert
        Assert.Equal(new SensorId(value), new SensorId(value));
    }
}
