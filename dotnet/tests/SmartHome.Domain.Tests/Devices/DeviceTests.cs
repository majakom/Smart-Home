using SmartHome.Domain.Devices;

namespace SmartHome.Domain.Tests.Devices;

public class DeviceTests
{
    [Fact]
    public void Constructor_CreatesEnabledDevice_WhenArgumentsAreValid()
    {
        // Arrange
        string name = "Device Name";
        string model = "Device Model";

        // Act
        Device device = new Device(name, model);

        // Assert
        Assert.False(device.Id.IsEmpty);
        Assert.Equal(name, device.Name);
        Assert.Equal(model, device.Model);
        Assert.Equal(DateTimeKind.Utc, device.RegisteredAtUtc.Kind);
        Assert.Null(device.LastSeenAtUtc);
        Assert.True(device.IsEnabled);
    }

    [Fact]
    public void Constructor_TrimsNameAndModel_WhenArgumentsContainWhiteSpace()
    {
        // Arrange
        string name = "  Device Name    ";
        string model = "  Device Model  ";

        // Act
        Device device = new Device(name, model);

        // Assert
        Assert.Equal("Device Name", device.Name);
        Assert.Equal("Device Model", device.Model);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsArgumentException_WhenNameIsInvalid(string? name)
    {
        // Act
        Action act = () => new Device(name!, "Device Model");

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsArgumentException_WhenModelIsInvalid(string? model)
    {
        // Act
        Action act = () => new Device("Device Name", model!);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("model", exception.ParamName);
    }

    [Fact]
    public void Rename_TrimsName()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");

        // Act
        device.Rename("   New Name  ");

        // Assert
        Assert.Equal("New Name", device.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_ThrowsArgumentException_WhenNameIsInvalid(string? name)
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        string originalName = device.Name;

        // Act
        Action act = () => device.Rename(name!);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("name", exception.ParamName);
        Assert.Equal(originalName, device.Name);
    }

    [Fact]
    public void ChangeModel_TrimsModel()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");

        // Act
        device.ChangeModel("   New Model  ");

        // Assert
        Assert.Equal("New Model", device.Model);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeModel_ThrowsArgumentException_WhenModelIsInvalid(string? model)
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        string originalModel = device.Model;

        // Act
        Action act = () => device.ChangeModel(model!);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("model", exception.ParamName);
        Assert.Equal(originalModel, device.Model);
    }

    [Fact]
    public void MarkAsSeen_SetsLastSeenAtUtc_WhenFirstTimestampIsValid()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        DateTime timestamp = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);

        // Act
        device.MarkAsSeen(timestamp);

        // Assert
        Assert.Equal(timestamp, device.LastSeenAtUtc);
    }

    [Fact]
    public void MarkAsSeen_UpdatesLastSeenAtUtc_WhenTimestampIsLater()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        DateTime firstTimestamp = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        DateTime laterTimestamp = new DateTime(2026, 6, 6, 12, 0, 1, DateTimeKind.Utc);

        device.MarkAsSeen(firstTimestamp);

        // Act
        device.MarkAsSeen(laterTimestamp);

        // Assert
        Assert.Equal(laterTimestamp, device.LastSeenAtUtc);
    }

    [Fact]
    public void MarkAsSeen_PreservesLastSeenAtUtc_WhenTimestampIsEarlier()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        DateTime firstTimestamp = new DateTime(2026, 6, 6, 12, 0, 1, DateTimeKind.Utc);
        DateTime earlierTimestamp = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);

        device.MarkAsSeen(firstTimestamp);

        // Act
        device.MarkAsSeen(earlierTimestamp);

        // Assert
        Assert.Equal(firstTimestamp, device.LastSeenAtUtc);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void MarkAsSeen_ThrowsArgumentException_WhenTimestampIsNotUtc(DateTimeKind kind)
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        DateTime timestamp = new DateTime(2026, 6, 6, 12, 0, 0, kind);

        // Act
        Action act = () => device.MarkAsSeen(timestamp);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("timestampUtc", exception.ParamName);
        Assert.Null(device.LastSeenAtUtc);
    }

    [Fact]
    public void Enable_SetsIsEnabledTrue()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        device.Disable();

        // Act
        device.Enable();

        // Assert
        Assert.True(device.IsEnabled);
    }

    [Fact]
    public void Disable_SetsIsEnabledFalse()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");

        // Act
        device.Disable();

        // Assert
        Assert.False(device.IsEnabled);
    }
}