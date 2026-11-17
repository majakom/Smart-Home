using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using SmartHome.Application;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.TelemetryWorker.Configuration;
using SmartHome.TelemetryWorker.HostedServices;
using SmartHome.TelemetryWorker.Mqtt.Connection;
using SmartHome.TelemetryWorker.Mqtt.Receiving;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry.Handlers;

namespace SmartHome.TelemetryWorker.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddTelemetryWorker_RegistersServicesWithCorrectLifetimes()
    {
        // Arrange
        ServiceCollection services = new();

        services.AddApplication();
        services.AddSingleton(Substitute.For<ISensorReader>());
        services.AddSingleton(Substitute.For<IDeviceReader>());
        services.AddSingleton(Substitute.For<IDevicePresenceWriter>());
        services.AddSingleton(Substitute.For<IClimateMeasurementWriter>());
        services.AddLogging();
        services.AddTelemetryWorker(CreateConfiguration());

        IHostEnvironment hostEnvironment = Substitute.For<IHostEnvironment>();
        hostEnvironment.ContentRootPath.Returns(AppContext.BaseDirectory);
        services.AddSingleton(hostEnvironment);

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
        IMqttMessageSubscriber subscriber =
            scope1.ServiceProvider.GetRequiredService<IMqttMessageSubscriber>();
        MqttMessageProcessor messageProcessor =
            scope1.ServiceProvider.GetRequiredService<MqttMessageProcessor>();
        TelemetryMessageDispatcher dispatcher =
            scope1.ServiceProvider.GetRequiredService<TelemetryMessageDispatcher>();
        TelemetryOutcomeLogger outcomeLogger =
            scope1.ServiceProvider.GetRequiredService<TelemetryOutcomeLogger>();
        MqttCertificateLoader certificateLoader =
            scope1.ServiceProvider.GetRequiredService<MqttCertificateLoader>();
        MqttClientFactory clientFactory =
            scope1.ServiceProvider.GetRequiredService<MqttClientFactory>();
        ITelemetryMessageHandler messageHandler =
            scope1.ServiceProvider.GetRequiredService<ITelemetryMessageHandler>();

        // Assert
        IHostedService hostedService =
            Assert.Single(serviceProvider.GetServices<IHostedService>());

        Assert.IsType<TelemetrySubscriber>(subscriber);
        Assert.IsType<ClimateMeasurementMessageHandler>(messageHandler);
        Assert.IsType<MqttHostedService>(hostedService);

        Assert.Same(
            subscriber,
            scope2.ServiceProvider.GetRequiredService<IMqttMessageSubscriber>()
        );
        Assert.Same(
            messageProcessor,
            scope2.ServiceProvider.GetRequiredService<MqttMessageProcessor>()
        );
        Assert.Same(
            dispatcher,
            scope2.ServiceProvider.GetRequiredService<TelemetryMessageDispatcher>()
        );
        Assert.Same(
            outcomeLogger,
            scope2.ServiceProvider.GetRequiredService<TelemetryOutcomeLogger>()
        );
        Assert.Same(
            certificateLoader,
            scope2.ServiceProvider.GetRequiredService<MqttCertificateLoader>()
        );
        Assert.Same(
            clientFactory,
            scope2.ServiceProvider.GetRequiredService<MqttClientFactory>()
        );

        Assert.NotSame(
            messageHandler,
            scope2.ServiceProvider.GetRequiredService<ITelemetryMessageHandler>()
        );
    }

    [Fact]
    public void AddTelemetryWorker_BindsAndValidatesMqttOptions()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddTelemetryWorker(CreateConfiguration());

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        MqttOptions options = serviceProvider.GetRequiredService<IOptions<MqttOptions>>().Value;

        // Assert
        Assert.Equal("broker.example", options.Host);
        Assert.Equal(8883, options.Port);
        Assert.Equal("telemetry-worker-tests", options.ClientId);
        Assert.Equal("test-user", options.Credentials.Username);
        Assert.Equal("test-password", options.Credentials.Password);
        Assert.Equal("ca.crt", options.Tls.CaCertificatePath);
        Assert.Equal("client.crt", options.Tls.ClientCertificatePath);
        Assert.Equal("client.key", options.Tls.ClientKeyPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddTelemetryWorker_ThrowsWhenMqttOptionsAreInvalid(string? host)
    {
        // Arrange
        ServiceCollection services = new();
        services.AddTelemetryWorker(CreateConfiguration(host!));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act


        // Act
        OptionsValidationException exception =
            Assert.Throws<OptionsValidationException>(
                () => serviceProvider.GetRequiredService<IOptions<MqttOptions>>().Value
            );

        // Assert
        Assert.Contains("MQTT host is required.", exception.Failures);
    }

    private static IConfiguration CreateConfiguration(string host = "broker.example")
    {
        Dictionary<string, string?> settings = new()
        {
            ["Mqtt:Host"] = host,
            ["Mqtt:Port"] = "8883",
            ["Mqtt:ClientId"] = "telemetry-worker-tests",
            ["Mqtt:KeepAliveSeconds"] = "30",
            ["Mqtt:ConnectionTimeoutSeconds"] = "10",
            ["Mqtt:ReconnectDelaySeconds"] = "5",
            ["Mqtt:Credentials:Username"] = "test-user",
            ["Mqtt:Credentials:Password"] = "test-password",
            ["Mqtt:Tls:CaCertificatePath"] = "ca.crt",
            ["Mqtt:Tls:ClientCertificatePath"] = "client.crt",
            ["Mqtt:Tls:ClientKeyPath"] = "client.key"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
