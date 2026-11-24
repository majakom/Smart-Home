using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Application.UseCases.Devices.GetDeviceById;
using SmartHome.Application.UseCases.Devices.GetDevices;
using SmartHome.Application.UseCases.Sensors.GetSensorById;
using SmartHome.Application.UseCases.Sensors.GetSensors;
using SmartHome.Application.UseCases.Telemetry.GetSensorClimateMeasurements;
using SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;

namespace SmartHome.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersServicesWithCorrectLifetimes()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddApplication();
        services.AddSingleton(Substitute.For<ISensorReader>());
        services.AddSingleton(Substitute.For<IDeviceReader>());
        services.AddSingleton(Substitute.For<IDevicePresenceWriter>());
        services.AddSingleton(Substitute.For<IClimateMeasurementReader>());
        services.AddSingleton(Substitute.For<IClimateMeasurementWriter>());

        using ServiceProvider serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            }
        );

        // Act
        using IServiceScope scope1 = serviceProvider.CreateScope();
        using IServiceScope scope2 = serviceProvider.CreateScope();
        
        TelemetryQueryValidator? telemetryQueryValidator =
            scope1.ServiceProvider.GetService<TelemetryQueryValidator>();

        TelemetryRequestValidator? telemetryRequestValidator = 
            scope1.ServiceProvider.GetService<TelemetryRequestValidator>();

        RecordClimateMeasurementRequestValidator? recordClimateMeasurementRequestValidator = 
            scope1.ServiceProvider.GetService<RecordClimateMeasurementRequestValidator>();

        GetDeviceByIdUseCase? getDeviceByIdUse =
            scope1.ServiceProvider.GetService<GetDeviceByIdUseCase>();

        GetDevicesUseCase? getDevicesUseCase =
            scope1.ServiceProvider.GetService<GetDevicesUseCase>();

        GetSensorByIdUseCase? getSensorByIdUseCase =
            scope1.ServiceProvider.GetService<GetSensorByIdUseCase>();

        GetSensorsUseCase? getSensorsUseCase =
            scope1.ServiceProvider.GetService<GetSensorsUseCase>();

        TelemetryQuerySensorValidator? telemetryQuerySensorValidator =
            scope1.ServiceProvider.GetService<TelemetryQuerySensorValidator>();

        GetSensorClimateMeasurementsUseCase? getSensorClimateMeasurementsUseCase =
            scope1.ServiceProvider.GetService<GetSensorClimateMeasurementsUseCase>();

        TelemetrySourceValidator? telemetrySourceValidator = 
            scope1.ServiceProvider.GetService<TelemetrySourceValidator>();

        RecordClimateMeasurementUseCase? recordClimateMeasurementUseCase = 
            scope1.ServiceProvider.GetService<RecordClimateMeasurementUseCase>();

        // Assert
        Assert.NotNull(telemetryQueryValidator);

        Assert.NotNull(telemetryRequestValidator);
        Assert.NotNull(recordClimateMeasurementRequestValidator);

        Assert.NotNull(getDeviceByIdUse);
        Assert.NotNull(getDevicesUseCase);

        Assert.NotNull(getSensorByIdUseCase);
        Assert.NotNull(getSensorsUseCase);

        Assert.NotNull(telemetryQuerySensorValidator);
        Assert.NotNull(getSensorClimateMeasurementsUseCase);
        
        Assert.NotNull(telemetrySourceValidator);
        Assert.NotNull(recordClimateMeasurementUseCase);

        Assert.Same(
            scope1.ServiceProvider.GetRequiredService<TimeProvider>(),
            scope2.ServiceProvider.GetRequiredService<TimeProvider>()
        );

        Assert.Same(
            telemetryQueryValidator,
            scope2.ServiceProvider.GetRequiredService<TelemetryQueryValidator>()
        );
        
        Assert.Same(
            telemetryRequestValidator,
            scope2.ServiceProvider.GetRequiredService<TelemetryRequestValidator>()
        );

        Assert.Same(
            recordClimateMeasurementRequestValidator,
            scope2.ServiceProvider.GetRequiredService<RecordClimateMeasurementRequestValidator>()
        );

        Assert.NotSame(
            getDeviceByIdUse,
            scope2.ServiceProvider.GetRequiredService<GetDeviceByIdUseCase>()
        );

        Assert.NotSame(
            getDevicesUseCase,
            scope2.ServiceProvider.GetRequiredService<GetDevicesUseCase>()
        );

        Assert.NotSame(
            getSensorByIdUseCase,
            scope2.ServiceProvider.GetRequiredService<GetSensorByIdUseCase>()
        );

        Assert.NotSame(
            getSensorsUseCase,
            scope2.ServiceProvider.GetRequiredService<GetSensorsUseCase>()
        );

        Assert.NotSame(
            telemetryQuerySensorValidator,
            scope2.ServiceProvider.GetRequiredService<TelemetryQuerySensorValidator>()
        );

        Assert.NotSame(
            getSensorClimateMeasurementsUseCase,
            scope2.ServiceProvider.GetRequiredService<GetSensorClimateMeasurementsUseCase>()
        );

        Assert.NotSame(
            telemetrySourceValidator,
            scope2.ServiceProvider.GetRequiredService<TelemetrySourceValidator>()
        );

        Assert.NotSame(
            recordClimateMeasurementUseCase,
            scope2.ServiceProvider.GetRequiredService<RecordClimateMeasurementUseCase>()
        );
    }
}
