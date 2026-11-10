using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;
using SmartHome.Domain.Shared;

namespace SmartHome.Domain.Tests.Sensors;

public class SensorTests
{
    [Fact]
    public void Constructor_CreatesEnabledSensor_WhenArgumentsAreValid()
    {
        // Arrange
        DeviceId deviceId = new(Guid.NewGuid());
        SensorCategory category = SensorCategory.Climate;
        string name = "Sensor Name";
        string model = "Sensor Model";
        string serialNumber = "SN-12345";

        // Act
        Sensor sensor = new(deviceId, category, name, model, serialNumber);

        // Assert
        Assert.False(sensor.Id.IsEmpty);
        Assert.Equal(deviceId, sensor.DeviceId);
        Assert.Equal(category, sensor.Category);
        Assert.Equal(name, sensor.Name);
        Assert.Equal(model, sensor.Model);
        Assert.Equal(SerialNumber.Parse(serialNumber), sensor.SerialNumber);
        Assert.Equal(DateTimeKind.Utc, sensor.RegisteredAtUtc.Kind);
        Assert.True(sensor.IsEnabled);
    }

    [Fact]
    public void Constructor_TrimsNameAndModel_WhenArgumentsContainWhiteSpace()
    {
        // Arrange
        DeviceId deviceId = new(Guid.NewGuid());

        // Act
        Sensor sensor = new(
            deviceId,
            SensorCategory.Climate,
            "  Sensor Name  ",
            "  Sensor Model  "
        );

        // Assert
        Assert.Equal("Sensor Name", sensor.Name);
        Assert.Equal("Sensor Model", sensor.Model);
    }

    [Fact]
    public void Constructor_CreatesSensorWithoutSerialNumber_WhenSerialNumberIsNull()
    {
        // Arrange
        DeviceId deviceId = new(Guid.NewGuid());

        // Act
        Sensor sensor = new(
            deviceId,
            SensorCategory.Climate,
            "Sensor Name",
            "Sensor Model"
        );

        // Assert
        Assert.Null(sensor.SerialNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_CreatesSensorWithoutSerialNumber_WhenSerialNumberIsEmptyOrWhitespace(
        string serialNumber
    )
    {
        // Arrange
        DeviceId deviceId = new(Guid.NewGuid());

        // Act
        Sensor sensor = new(
            deviceId,
            SensorCategory.Climate,
            "Sensor Name",
            "Sensor Model",
            serialNumber
        );

        // Assert
        Assert.Null(sensor.SerialNumber);
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenDeviceIdIsEmpty()
    {
        // Act
        Action act = () => new Sensor(
            default,
            SensorCategory.Climate,
            "Sensor Name",
            "Sensor Model"
        );

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("deviceId", exception.ParamName);
    }

    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenCategoryIsInvalid()
    {
        // Arrange
        DeviceId deviceId = new(Guid.NewGuid());
        SensorCategory category = (SensorCategory)999;

        // Act
        Action act = () => new Sensor(
            deviceId,
            category,
            "Sensor Name",
            "Sensor Model"
        );

        // Assert
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("category", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsArgumentException_WhenNameIsInvalid(string? name)
    {
        // Arrange
        DeviceId deviceId = new(Guid.NewGuid());

        // Act
        Action act = () => new Sensor(
            deviceId,
            SensorCategory.Climate,
            name!,
            "Sensor Model"
        );

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
        // Arrange
        DeviceId deviceId = new(Guid.NewGuid());

        // Act
        Action act = () => new Sensor(
            deviceId,
            SensorCategory.Climate,
            "Sensor Name",
            model!
        );

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("model", exception.ParamName);
    }

    [Fact]
    public void Rename_TrimsName()
    {
        // Arrange
        Sensor sensor = CreateSensor();

        // Act
        sensor.Rename("  New Name  ");

        // Assert
        Assert.Equal("New Name", sensor.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_ThrowsArgumentException_WhenNameIsInvalid(string? name)
    {
        // Arrange
        Sensor sensor = CreateSensor();
        string originalName = sensor.Name;

        // Act
        Action act = () => sensor.Rename(name!);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("name", exception.ParamName);
        Assert.Equal(originalName, sensor.Name);
    }

    [Fact]
    public void ChangeModel_TrimsModel()
    {
        // Arrange
        Sensor sensor = CreateSensor();

        // Act
        sensor.ChangeModel("  New Model  ");

        // Assert
        Assert.Equal("New Model", sensor.Model);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeModel_ThrowsArgumentException_WhenModelIsInvalid(string? model)
    {
        // Arrange
        Sensor sensor = CreateSensor();
        string originalModel = sensor.Model;

        // Act
        Action act = () => sensor.ChangeModel(model!);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("model", exception.ParamName);
        Assert.Equal(originalModel, sensor.Model);
    }

    [Fact]
    public void ChangeSerialNumber_UpdatesSerialNumber_WhenValueIsValid()
    {
        // Arrange
        Sensor sensor = CreateSensor();
        string serialNumber = "SN-67890";

        // Act
        sensor.ChangeSerialNumber(serialNumber);

        // Assert
        Assert.Equal(SerialNumber.Parse(serialNumber), sensor.SerialNumber);
    }

    [Fact]
    public void ChangeSerialNumber_SetsSerialNumberToNull_WhenValueIsNull()
    {
        // Arrange
        Sensor sensor = CreateSensor("SN-12345");

        // Act
        sensor.ChangeSerialNumber(null);

        // Assert
        Assert.Null(sensor.SerialNumber);
    }

    [Fact]
    public void Enable_SetsIsEnabledTrue()
    {
        // Arrange
        Sensor sensor = CreateSensor();
        sensor.Disable();

        // Act
        sensor.Enable();

        // Assert
        Assert.True(sensor.IsEnabled);
    }

    [Fact]
    public void Disable_SetsIsEnabledFalse()
    {
        // Arrange
        Sensor sensor = CreateSensor();

        // Act
        sensor.Disable();

        // Assert
        Assert.False(sensor.IsEnabled);
    }

    private static Sensor CreateSensor(string? serialNumber = null)
    {
        return new Sensor(
            new DeviceId(Guid.NewGuid()),
            SensorCategory.Climate,
            "Sensor Name",
            "Sensor Model",
            serialNumber
        );
    }
}