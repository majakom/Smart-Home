using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Tests.Mqtt.Topics.Telemetry;

public class TelemetryTopicParserTests
{
    [Fact]
    public void TryParse_ReturnsTelemetryTopic_WhenTopicIsValid()
    {
        // Arrange
        Guid deviceGuid = Guid.NewGuid();
        Guid sensorGuid = Guid.NewGuid();

        string topic =
            $"smarthome/devices/{deviceGuid:D}/sensors/{sensorGuid:D}/telemetry/climate";

        // Act
        bool result = TelemetryTopicParser.TryParse(
            topic,
            out TelemetryTopic? telemetryTopic,
            out TelemetryTopicParseFailure failure
        );

        // Assert
        Assert.True(result);
        Assert.Equal(TelemetryTopicParseFailure.None, failure);
        Assert.NotNull(telemetryTopic);

        Assert.Equal(new DeviceId(deviceGuid), telemetryTopic.DeviceId);
        Assert.Equal(new SensorId(sensorGuid), telemetryTopic.SensorId);
        Assert.Equal(SensorCategory.Climate, telemetryTopic.Category);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void TryParse_ReturnsEmptyTopic_WhenTopicIsEmpty(string? topic)
    {
        // Act
        bool result = TelemetryTopicParser.TryParse(
            topic,
            out TelemetryTopic? telemetryTopic,
            out TelemetryTopicParseFailure failure
        );

        // Assert
        Assert.False(result);
        Assert.Null(telemetryTopic);
        Assert.Equal(TelemetryTopicParseFailure.EmptyTopic, failure);
    }

    [Theory]
    [InlineData("smarthome/devices")]
    [InlineData("smarthome/devices/{0}/sensors/{1}/telemetry")]
    [InlineData("smarthome/devices/{0}/sensors/{1}/telemetry/climate/extra")]
    [InlineData("wrong/devices/{0}/sensors/{1}/telemetry/climate")]
    [InlineData("smarthome/wrong/{0}/sensors/{1}/telemetry/climate")]
    [InlineData("smarthome/devices/{0}/wrong/{1}/telemetry/climate")]
    [InlineData("smarthome/devices/{0}/sensors/{1}/wrong/climate")]
    public void TryParse_ReturnsInvalidShape_WhenTopicStructureIsInvalid(string topicTemplate)
    {
        // Arrange
        Guid deviceGuid = Guid.NewGuid();
        Guid sensorGuid = Guid.NewGuid();

        string topic = string.Format(
            topicTemplate,
            deviceGuid.ToString("D"),
            sensorGuid.ToString("D")
        );

        // Act
        bool result = TelemetryTopicParser.TryParse(
            topic,
            out TelemetryTopic? telemetryTopic,
            out TelemetryTopicParseFailure failure
        );

        // Assert
        Assert.False(result);
        Assert.Null(telemetryTopic);
        Assert.Equal(TelemetryTopicParseFailure.InvalidShape, failure);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void TryParse_ReturnsInvalidDeviceId_WhenDeviceIdIsInvalid(
        string deviceId)
    {
        // Arrange
        string topic = CreateTopic(
            deviceId,
            Guid.NewGuid().ToString("D")
        );

        // Act
        bool result = TelemetryTopicParser.TryParse(
            topic,
            out TelemetryTopic? telemetryTopic,
            out TelemetryTopicParseFailure failure
        );

        // Assert
        Assert.False(result);
        Assert.Null(telemetryTopic);
        Assert.Equal(TelemetryTopicParseFailure.InvalidDeviceId, failure);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void TryParse_ReturnsInvalidSensorId_WhenSensorIdIsInvalid(
        string sensorId)
    {
        // Arrange
        string topic = CreateTopic(
            Guid.NewGuid().ToString("D"),
            sensorId
        );

        // Act
        bool result = TelemetryTopicParser.TryParse(
            topic,
            out TelemetryTopic? telemetryTopic,
            out TelemetryTopicParseFailure failure
        );

        // Assert
        Assert.False(result);
        Assert.Null(telemetryTopic);
        Assert.Equal(TelemetryTopicParseFailure.InvalidSensorId, failure);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("Climate")]
    [InlineData("CLIMATE")]
    [InlineData("")]
    public void TryParse_ReturnsUnsupportedCategory_WhenCategoryIsInvalid(
        string category)
    {
        // Arrange
        string topic = CreateTopic(
            Guid.NewGuid().ToString("D"),
            Guid.NewGuid().ToString("D"),
            category
        );

        // Act
        bool result = TelemetryTopicParser.TryParse(
            topic,
            out TelemetryTopic? telemetryTopic,
            out TelemetryTopicParseFailure failure
        );

        // Assert
        Assert.False(result);
        Assert.Null(telemetryTopic);
        Assert.Equal(TelemetryTopicParseFailure.UnsupportedCategory, failure);
    }

    private static string CreateTopic(
        string deviceId,
        string sensorId,
        string category = "climate")
    {
        return
            $"smarthome/devices/{deviceId}/sensors/{sensorId}/telemetry/{category}";
    }
}
