using SmartHome.Domain.Measurements;

namespace SmartHome.Domain.Tests.Measurements;

public class ClimateMeasurementIdTests
{
    [Fact]
    public void Constructor_PreservesProvidedValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        ClimateMeasurementId measurementId = new(value);

        // Assert
        Assert.Equal(value, measurementId.Value);
    }

    [Fact]
    public void New_ReturnsNonEmptyId()
    {
        // Act
        ClimateMeasurementId measurementId = ClimateMeasurementId.New();

        // Assert
        Assert.False(measurementId.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsTrue_WhenValueIsEmpty()
    {
        // Arrange
        ClimateMeasurementId measurementId = new(Guid.Empty);

        // Act & Assert
        Assert.True(measurementId.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalse_WhenValueIsNotEmpty()
    {
        // Arrange
        ClimateMeasurementId measurementId = new(Guid.NewGuid());

        // Act & Assert
        Assert.False(measurementId.IsEmpty);
    }

    [Fact]
    public void Default_ReturnsEmptyId()
    {
        // Act
        ClimateMeasurementId measurementId = default;

        // Assert
        Assert.True(measurementId.IsEmpty);
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenValuesMatch()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act & Assert
        Assert.Equal(new ClimateMeasurementId(value), new ClimateMeasurementId(value));
    }
}