using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Tests.Mqtt.Topics.Telemetry;

public class TelemetryTopicFiltersTests
{
    [Fact]
    public void All_ReturnsExpectedWildcardFilter()
    {
        // Arrange
        string expected = "smarthome/devices/+/sensors/+/telemetry/+";

        // Act
        string result = TelemetryTopicFilters.All;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ForCategory_ReturnsClimateFilter_WhenCategoryIsClimate()
    {
        // Arrange
        SensorCategory category = SensorCategory.Climate;
        string expected = "smarthome/devices/+/sensors/+/telemetry/climate";

        // Act
        string result = TelemetryTopicFilters.ForCategory(category);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ForCategory_ThrowsArgumentOutOfRangeException_WhenCategoryIsUnsupported()
    {
        // Arrange
        SensorCategory category = (SensorCategory)999;

        // Act
        Func<string> act = () => TelemetryTopicFilters.ForCategory(category);

        // Assert
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("category", exception.ParamName);
    }
}
