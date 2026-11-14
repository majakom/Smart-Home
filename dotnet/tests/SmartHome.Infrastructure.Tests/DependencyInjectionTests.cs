using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Infrastructure.Persistence;
using SmartHome.Infrastructure.Persistence.Adapters.Devices;
using SmartHome.Infrastructure.Persistence.Adapters.Sensors;
using SmartHome.Infrastructure.Persistence.Adapters.Telemetry;

namespace SmartHome.Infrastructure.Tests;

public class DependencyInjectionTests
{
    private const string ConnectionString =
        "Host=localhost;Database=smarthome_tests;Username=test;Password=test";

    [Fact]
    public void AddInfrastructure_RegistersServicesWithScopedLifetime()
    {
        // Arrange
        ServiceCollection services = new();

        services.AddInfrastructure(ConnectionString);

        using ServiceProvider serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            }
        );

        using IServiceScope scope1 = serviceProvider.CreateScope();
        using IServiceScope scope2 = serviceProvider.CreateScope();

        // Act
        SmartHomeDbContext dbContext =
            scope1.ServiceProvider.GetRequiredService<SmartHomeDbContext>();

        IDeviceReader deviceReader =
            scope1.ServiceProvider.GetRequiredService<IDeviceReader>();

        ISensorReader sensorReader =
            scope1.ServiceProvider.GetRequiredService<ISensorReader>();

        IClimateMeasurementWriter measurementWriter =
            scope1.ServiceProvider.GetRequiredService<IClimateMeasurementWriter>();

        // Assert
        Assert.IsType<EfDeviceReader>(deviceReader);
        Assert.IsType<EfSensorReader>(sensorReader);
        Assert.IsType<EfClimateMeasurementWriter>(measurementWriter);

        Assert.Same(
            dbContext,
            scope1.ServiceProvider.GetRequiredService<SmartHomeDbContext>()
        );

        Assert.Same(
            deviceReader,
            scope1.ServiceProvider.GetRequiredService<IDeviceReader>()
        );

        Assert.Same(
            sensorReader,
            scope1.ServiceProvider.GetRequiredService<ISensorReader>()
        );

        Assert.Same(
            measurementWriter,
            scope1.ServiceProvider.GetRequiredService<IClimateMeasurementWriter>()
        );

        Assert.NotSame(
            dbContext,
            scope2.ServiceProvider.GetRequiredService<SmartHomeDbContext>()
        );

        Assert.NotSame(
            deviceReader,
            scope2.ServiceProvider.GetRequiredService<IDeviceReader>()
        );

        Assert.NotSame(
            sensorReader,
            scope2.ServiceProvider.GetRequiredService<ISensorReader>()
        );

        Assert.NotSame(
            measurementWriter,
            scope2.ServiceProvider.GetRequiredService<IClimateMeasurementWriter>()
        );
    }

    [Fact]
    public void AddInfrastructure_ConfiguresPostgreSqlProvider()
    {
        // Arrange
        ServiceCollection services = new();

        services.AddInfrastructure(ConnectionString);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();

        // Act
        SmartHomeDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<SmartHomeDbContext>();

        // Assert
        Assert.True(dbContext.Database.IsNpgsql());
    }

    [Fact]
    public void AddInfrastructure_ThrowsArgumentNullExceptionWhenConnectionStringIsNull()
    {
        // Arrange
        ServiceCollection services = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => services.AddInfrastructure(null!)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AddInfrastructure_ThrowsWhenConnectionStringIsInvalid(
        string? connectionString)
    {
        // Arrange
        ServiceCollection services = new();

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => services.AddInfrastructure(connectionString!)
        );
    }
}
