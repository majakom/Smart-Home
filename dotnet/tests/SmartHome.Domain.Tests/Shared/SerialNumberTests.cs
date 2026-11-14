using SmartHome.Domain.Shared;

namespace SmartHome.Domain.Tests.Shared;

public class SerialNumberTests
{
    [Fact]
    public void Parse_ReturnsSerialNumber_WhenValueIsValid()
    {
        // Arrange
        string value = "SN-12345";

        // Act
        SerialNumber? serialNumber = SerialNumber.Parse(value);

        // Assert
        Assert.NotNull(serialNumber);
        Assert.Equal(value, serialNumber.Value);
    }

    [Fact]
    public void Parse_TrimsValue_WhenValueContainsWhiteSpace()
    {
        // Arrange
        string value = "  SN-12345  ";

        // Act
        SerialNumber? serialNumber = SerialNumber.Parse(value);

        // Assert
        Assert.NotNull(serialNumber);
        Assert.Equal("SN-12345", serialNumber.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_ReturnsNull_WhenValueIsNullOrWhiteSpace(string? value)
    {
        // Act
        SerialNumber? serialNumber = SerialNumber.Parse(value);

        // Assert
        Assert.Null(serialNumber);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        SerialNumber serialNumber = SerialNumber.Parse("SN-12345")!;

        // Act
        string result = serialNumber.ToString();

        // Assert
        Assert.Equal("SN-12345", result);
    }
}