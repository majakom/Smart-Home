
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MQTTnet;
using SmartHome.TelemetryWorker.Configuration;
using SmartHome.TelemetryWorker.Mqtt.Connection;
using TelemetryMqttClientFactory = SmartHome.TelemetryWorker.Mqtt.Connection.MqttClientFactory;

namespace SmartHome.TelemetryWorker.Tests.Mqtt.Connection;

public class MqttClientFactoryTests
{
    [Fact]
    public void CreateOptions_UsesConfiguredConnectionAndTlsSettings()
    {
        // Arrange
        using MqttTlsCertificates certificates = new(
            CreateCertificate("CN=Test CA"),
            CreateCertificate("CN=Test Client")
        );

        MqttOptions mqttOptions = new()
        {
            Host = "broker.example.test",
            Port = 8883,
            ClientId = "test-worker",
            KeepAliveSeconds = 30,
            ConnectionTimeoutSeconds = 10,
            Credentials = new CredentialsOptions
            {
                Username = "test-user",
                Password = "test-password"
            }
        };

        // Act
        MqttClientOptions options = new TelemetryMqttClientFactory()
            .CreateOptions(mqttOptions, certificates);

        // Assert
        Assert.Equal("test-worker", options.ClientId);
        Assert.Equal(TimeSpan.FromSeconds(30), options.KeepAlivePeriod);
        Assert.Equal(TimeSpan.FromSeconds(10), options.Timeout);

        MqttClientTcpOptions tcpOptions =
            Assert.IsType<MqttClientTcpOptions>(options.ChannelOptions);

        DnsEndPoint endpoint =
            Assert.IsType<DnsEndPoint>(tcpOptions.RemoteEndpoint);

        Assert.Equal("broker.example.test", endpoint.Host);
        Assert.Equal(8883, endpoint.Port);

        Assert.NotNull(options.Credentials);

        MqttClientTlsOptions tlsOptions = tcpOptions.TlsOptions;

        Assert.True(tlsOptions.UseTls);
        Assert.Equal(SslProtocols.Tls12, tlsOptions.SslProtocol);
        Assert.Contains(certificates.CaCertificate, tlsOptions.TrustChain);

        Assert.NotNull(tlsOptions.ClientCertificatesProvider);
        Assert.Contains(
            certificates.ClientCertificate,
            tlsOptions.ClientCertificatesProvider
                .GetCertificates()
                .Cast<X509Certificate2>()
        );
    }

    [Fact]
    public void CreateOptions_ThrowsArgumentNullException_WhenOptionsAreNull()
    {
        // Arrange
        using MqttTlsCertificates certificates = new(
            CreateCertificate("CN=Test CA"),
            CreateCertificate("CN=Test Client")
        );

        // Act
        Func<MqttClientOptions> act = () => new TelemetryMqttClientFactory().CreateOptions(null!, certificates);
        
        // Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("options", exception.ParamName);
    }

    [Fact]
    public void CreateOptions_ThrowsArgumentNullException_WhenCertificatesAreNull()
    {
        // Arrange
        MqttOptions options = new()
        {
            Host = "broker.example.test",
            Credentials = new CredentialsOptions
            {
                Username = "test-user",
                Password = "test-password"
            }
        };

        // Act
        Func<MqttClientOptions> act = () => new TelemetryMqttClientFactory().CreateOptions(options, null!);

        // Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("certificates", exception.ParamName);
    }

    [Fact]
    public void CreateClient_ReturnsMqttClient()
    {
        // Arrange
        TelemetryMqttClientFactory factory = new();

        // Act
        using IMqttClient client = factory.CreateClient();

        // Assert
        Assert.NotNull(client);
    }

    private static X509Certificate2 CreateCertificate(string subject)
    {
        using RSA key = RSA.Create(2048);

        CertificateRequest request = new(
            subject,
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        return request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1)
        );
    }
}
