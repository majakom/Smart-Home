using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Tests.Mqtt.Topics.Telemetry;

public sealed class MqttTelemetryCategoriesTests
{
    [Fact]
    public void TryParse_ReturnsTrue_WhenValueIsClimate()
    {
        // Arrange
        string value = "climate";

        // Act
        bool result = MqttTelemetryCategories.TryParse(
            value,
            out SensorCategory category
        );

        // Assert
        Assert.True(result);
        Assert.Equal(SensorCategory.Climate, category);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("unknown")]
    [InlineData("")]
    [InlineData("Climate")]
    [InlineData("CLIMATE")]
    public void TryParse_ReturnsFalse_WhenCategoryIsUnsupported(string? value)
    {
        // Act
        bool result = MqttTelemetryCategories.TryParse(
            value!,
            out SensorCategory category
        );

        // Assert
        Assert.False(result);
        Assert.Equal(default, category);
    }

    [Fact]
    public void GetName_ReturnsClimate_WhenCategoryIsClimate()
    {
        // Arrange
        SensorCategory category = SensorCategory.Climate;

        // Act
        string result = MqttTelemetryCategories.GetName(category);

        // Assert
        Assert.Equal("climate", result);
    }

    [Fact]
    public void GetName_ThrowsArgumentOutOfRangeException_WhenCategoryIsUnsupported()
    {
        // Arrange
        SensorCategory category = (SensorCategory)999;

        // Act
        Action act = () => MqttTelemetryCategories.GetName(category);

        // Assert
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("category", exception.ParamName);
    }
}
