
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartHome.Domain.Devices;

namespace SmartHome.Infrastructure.Tests.Persistence.Mappings;

public sealed class DeviceMappingTests : PersistenceMappingTestBase
{
    [Fact]
    public void Model_MapsDeviceToExpectedTable()
    {
        // Assert
        Assert.Equal("devices", GetEntityType<Device>().GetTableName());
    }

    [Fact]
    public void Model_MapsDeviceIdAsPrimaryKey()
    {
        // Assert
        AssertPrimaryKey<Device>(nameof(Device.Id));
    }

    [Fact]
    public void Model_MapsDeviceIdToGuid()
    {
        // Assert
        AssertProviderType<Device>(
            nameof(Device.Id),
            typeof(Guid)
        );
    }

    [Fact]
    public void Model_MapsExpectedDevicePropertyConstraints()
    {
        // Arrange
        IProperty name = GetProperty<Device>(nameof(Device.Name));
        IProperty model = GetProperty<Device>(nameof(Device.Model));
        IProperty registeredAtUtc =
            GetProperty<Device>(nameof(Device.RegisteredAtUtc));
        IProperty lastSeenAtUtc =
            GetProperty<Device>(nameof(Device.LastSeenAtUtc));
        IProperty isEnabled =
            GetProperty<Device>(nameof(Device.IsEnabled));

        // Assert
        Assert.False(name.IsNullable);
        Assert.Equal(100, name.GetMaxLength());

        Assert.False(model.IsNullable);
        Assert.Equal(100, model.GetMaxLength());

        Assert.False(registeredAtUtc.IsNullable);
        Assert.True(lastSeenAtUtc.IsNullable);
        Assert.False(isEnabled.IsNullable);
    }
}
