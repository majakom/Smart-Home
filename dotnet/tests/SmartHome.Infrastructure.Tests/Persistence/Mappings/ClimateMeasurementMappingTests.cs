
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;
using SmartHome.Infrastructure.Persistence;

namespace SmartHome.Infrastructure.Tests.Persistence.Mappings;

public sealed class ClimateMeasurementMappingTests
    : PersistenceMappingTestBase
{
    [Fact]
    public void Model_MapsClimateMeasurementToExpectedTable()
    {
        // Assert
        Assert.Equal(
            "climate_measurements",
            GetEntityType<ClimateMeasurement>().GetTableName()
        );
    }

    [Fact]
    public void Model_MapsClimateMeasurementIdAsPrimaryKey()
    {
        // Assert
        AssertPrimaryKey<ClimateMeasurement>(
            nameof(ClimateMeasurement.Id)
        );
    }

    [Fact]
    public void Model_MapsMeasurementValueObjectsToExpectedProviderTypes()
    {
        // Assert
        AssertProviderType<ClimateMeasurement>(
            nameof(ClimateMeasurement.Id),
            typeof(Guid)
        );

        AssertProviderType<ClimateMeasurement>(
            nameof(ClimateMeasurement.SourceMessageId),
            typeof(long)
        );

        AssertProviderType<ClimateMeasurement>(
            nameof(ClimateMeasurement.SensorId),
            typeof(Guid)
        );
    }

    [Fact]
    public void Model_MapsSensorRelationshipWithRestrictDeleteBehavior()
    {
        // Arrange
        IForeignKey foreignKey =
            GetForeignKey<ClimateMeasurement>(
                nameof(ClimateMeasurement.SensorId)
            );

        // Assert
        Assert.Equal(
            typeof(Sensor),
            foreignKey.PrincipalEntityType.ClrType
        );

        Assert.True(foreignKey.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Model_MapsExpectedMeasurementPropertyConstraints()
    {
        // Arrange
        IProperty sourceMessageId =
            GetProperty<ClimateMeasurement>(
                nameof(ClimateMeasurement.SourceMessageId)
            );

        IProperty measuredAtUtc =
            GetProperty<ClimateMeasurement>(
                nameof(ClimateMeasurement.MeasuredAtUtc)
            );

        IProperty receivedAtUtc =
            GetProperty<ClimateMeasurement>(
                nameof(ClimateMeasurement.ReceivedAtUtc)
            );

        // Assert
        Assert.False(sourceMessageId.IsNullable);
        Assert.False(measuredAtUtc.IsNullable);
        Assert.False(receivedAtUtc.IsNullable);
    }

    [Fact]
    public void Model_MapsExpectedMeasurementPrecision()
    {
        // Arrange
        IProperty temperature =
            GetProperty<ClimateMeasurement>(
                nameof(ClimateMeasurement.TemperatureCelsius)
            );

        IProperty humidity =
            GetProperty<ClimateMeasurement>(
                nameof(ClimateMeasurement.RelativeHumidityPercent)
            );

        // Assert
        Assert.Equal(6, temperature.GetPrecision());
        Assert.Equal(2, temperature.GetScale());

        Assert.Equal(5, humidity.GetPrecision());
        Assert.Equal(2, humidity.GetScale());
    }

    [Fact]
    public void Model_MapsExpectedMeasurementIndexes()
    {
        // Arrange
        IEntityType measurement =
            GetEntityType<ClimateMeasurement>();

        // Act
        IIndex uniqueIndex = Assert.Single(
        measurement.GetIndexes(),
        index => index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual(new[]
                {
                    nameof(ClimateMeasurement.SensorId),
                    nameof(ClimateMeasurement.SourceMessageId)
                }));

        IIndex measuredAtIndex = Assert.Single(
            measurement.GetIndexes(),
            index => index.Properties.Select(property => property.Name)
                .SequenceEqual(
                    new[]
                    {
                        nameof(ClimateMeasurement.SensorId),
                        nameof(ClimateMeasurement.MeasuredAtUtc)
                    }
                )
        );

        // Assert
        Assert.Equal(
            new[]
            {
                nameof(ClimateMeasurement.SensorId),
                nameof(ClimateMeasurement.SourceMessageId)
            },
            uniqueIndex.Properties.Select(property => property.Name)
        );

        Assert.False(measuredAtIndex.IsUnique);
    }
}
