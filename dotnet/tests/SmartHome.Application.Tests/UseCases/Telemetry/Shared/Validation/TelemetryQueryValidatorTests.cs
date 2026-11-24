using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;

namespace SmartHome.Application.Tests.UseCases.Telemetry.Shared.Validation;

public class TelemetryQueryValidatorTests
{
    private readonly TelemetryQueryValidator _validator = new();

    [Fact]
    public void Validate_ReturnsNull_WhenRequestIsValid()
    {
        // Arrange
        DateTime fromUtc = ValidTimestamp();
        DateTime toUtc = fromUtc.AddHours(1);

        // Act
        TelemetryQueryFailureReason? result = _validator.Validate(
            fromUtc,
            toUtc,
            100
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Validate_ReturnsInvalidFromUtc_WhenFromUtcIsDefault()
    {
        // Arrange
        DateTime fromUtc = default;

        // Act
        TelemetryQueryFailureReason? result = _validator.Validate(
            fromUtc,
            ValidTimestamp(),
            100
        );

        // Assert
        Assert.Equal(TelemetryQueryFailureReason.InvalidFromUtc, result);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Validate_ReturnsInvalidFromUtc_WhenFromUtcIsNotUtc(DateTimeKind kind)
    {
        // Arrange
        DateTime fromUtc = new(2026, 6, 6, 12, 0, 0, kind);

        // Act
        TelemetryQueryFailureReason? result = _validator.Validate(
            fromUtc,
            ValidTimestamp().AddHours(1),
            100
        );

        // Assert
        Assert.Equal(TelemetryQueryFailureReason.InvalidFromUtc, result);
    }

    [Fact]
    public void Validate_ReturnsInvalidToUtc_WhenToUtcIsDefault()
    {
        // Arrange
        DateTime toUtc = default;

        // Act
        TelemetryQueryFailureReason? result = _validator.Validate(
            ValidTimestamp(),
            toUtc,
            100
        );

        // Assert
        Assert.Equal(TelemetryQueryFailureReason.InvalidToUtc, result);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Validate_ReturnsInvalidToUtc_WhenToUtcIsNotUtc(
        DateTimeKind kind)
    {
        // Arrange
        DateTime toUtc = new(2026, 6, 6, 13, 0, 0, kind);

        // Act
        TelemetryQueryFailureReason? result = _validator.Validate(
            ValidTimestamp(),
            toUtc,
            100
        );

        // Assert
        Assert.Equal(TelemetryQueryFailureReason.InvalidToUtc, result);
    }

    [Fact]
    public void Validate_ReturnsInvalidTimeRange_WhenFromUtcIsAfterToUtc()
    {
        // Arrange
        DateTime fromUtc = ValidTimestamp().AddHours(1);
        DateTime toUtc = ValidTimestamp();

        // Act
        TelemetryQueryFailureReason? result = _validator.Validate(
            fromUtc,
            toUtc,
            100
        );

        // Assert
        Assert.Equal(TelemetryQueryFailureReason.InvalidTimeRange, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(501)]
    [InlineData(1000)]
    public void Validate_ReturnsInvalidLimit_WhenLimitIsOutOfRange(int limit)
    {
        // Act
        TelemetryQueryFailureReason? result = _validator.Validate(
            ValidTimestamp(),
            ValidTimestamp().AddHours(1),
            limit
        );

        // Assert
        Assert.Equal(TelemetryQueryFailureReason.InvalidLimit, result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    public void Validate_ReturnsNull_WhenLimitIsWithinAllowedRange(int limit)
    {
        // Act
        TelemetryQueryFailureReason? result = _validator.Validate(
            ValidTimestamp(),
            ValidTimestamp().AddHours(1),
            limit
        );

        // Assert
        Assert.Null(result);
    }

    private static DateTime ValidTimestamp()
    {
        return new(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
    }
}
