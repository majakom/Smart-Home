using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.Abstractions.Telemetry;
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

        RecordClimateMeasurementUseCase? useCase = 
            scope1.ServiceProvider.GetService<RecordClimateMeasurementUseCase>();

        TelemetrySourceValidator? sourceValidator = 
            scope1.ServiceProvider.GetService<TelemetrySourceValidator>();

        RecordClimateMeasurementRequestValidator? requestValidator = 
            scope1.ServiceProvider.GetService<RecordClimateMeasurementRequestValidator>();
        
        TelemetryRequestValidator? telemetryRequestValidator = 
            scope1.ServiceProvider.GetService<TelemetryRequestValidator>();

        // Assert
        Assert.NotNull(useCase);
        Assert.NotNull(sourceValidator);
        Assert.NotNull(requestValidator);
        Assert.NotNull(telemetryRequestValidator);

        Assert.Same(
            telemetryRequestValidator,
            scope2.ServiceProvider.GetRequiredService<TelemetryRequestValidator>()
        );

        Assert.Same(
            requestValidator,
            scope2.ServiceProvider.GetRequiredService<RecordClimateMeasurementRequestValidator>()
        );

        Assert.Same(
            scope1.ServiceProvider.GetRequiredService<TimeProvider>(),
            scope2.ServiceProvider.GetRequiredService<TimeProvider>()
        );

        Assert.NotSame(
            sourceValidator,
            scope2.ServiceProvider.GetRequiredService<TelemetrySourceValidator>()
        );

        Assert.NotSame(
            useCase,
            scope2.ServiceProvider.GetRequiredService<RecordClimateMeasurementUseCase>()
        );
    }
}
