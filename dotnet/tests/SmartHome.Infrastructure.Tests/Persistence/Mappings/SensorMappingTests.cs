
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;

namespace SmartHome.Infrastructure.Tests.Persistence.Mappings;

public sealed class SensorMappingTests : PersistenceMappingTestBase
{
    [Fact]
    public void Model_MapsSensorToExpectedTable()
    {
        // Assert
        Assert.Equal("sensors", GetEntityType<Sensor>().GetTableName());
    }

    [Fact]
    public void Model_MapsSensorIdAsPrimaryKey()
    {
        // Assert
        AssertPrimaryKey<Sensor>(nameof(Sensor.Id));
    }

    [Fact]
    public void Model_MapsSensorValueObjectsToExpectedProviderTypes()
    {
        // Assert
        AssertProviderType<Sensor>(
            nameof(Sensor.Id),
            typeof(Guid)
        );

        AssertProviderType<Sensor>(
            nameof(Sensor.DeviceId),
            typeof(Guid)
        );

        AssertProviderType<Sensor>(
            nameof(Sensor.Category),
            typeof(string)
        );

        AssertProviderType<Sensor>(
            nameof(Sensor.SerialNumber),
            typeof(string)
        );
    }

    [Fact]
    public void Model_MapsDeviceRelationshipWithRestrictDeleteBehavior()
    {
        // Arrange
        IForeignKey foreignKey =
            GetForeignKey<Sensor>(nameof(Sensor.DeviceId));

        // Assert
        Assert.Equal(
            typeof(Device),
            foreignKey.PrincipalEntityType.ClrType
        );

        Assert.True(foreignKey.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Model_MapsExpectedSensorPropertyConstraints()
    {
        // Arrange
        IProperty category = GetProperty<Sensor>(nameof(Sensor.Category));
        IProperty name = GetProperty<Sensor>(nameof(Sensor.Name));
        IProperty model = GetProperty<Sensor>(nameof(Sensor.Model));
        IProperty serialNumber =
            GetProperty<Sensor>(nameof(Sensor.SerialNumber));
        IProperty registeredAtUtc =
            GetProperty<Sensor>(nameof(Sensor.RegisteredAtUtc));
        IProperty isEnabled =
            GetProperty<Sensor>(nameof(Sensor.IsEnabled));

        // Assert
        Assert.False(category.IsNullable);
        Assert.Equal(50, category.GetMaxLength());

        Assert.False(name.IsNullable);
        Assert.Equal(100, name.GetMaxLength());

        Assert.False(model.IsNullable);
        Assert.Equal(100, model.GetMaxLength());

        Assert.True(serialNumber.IsNullable);
        Assert.Equal(100, serialNumber.GetMaxLength());

        Assert.False(registeredAtUtc.IsNullable);
        Assert.False(isEnabled.IsNullable);
    }
}
