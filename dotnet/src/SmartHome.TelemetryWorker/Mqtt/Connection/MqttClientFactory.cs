using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using MQTTnet;
using SmartHome.TelemetryWorker.Configuration;

namespace SmartHome.TelemetryWorker.Mqtt.Connection;

public class MqttClientFactory
{
    public IMqttClient CreateClient()
    {
        MQTTnet.MqttClientFactory factory = new();
        return factory.CreateMqttClient();
    }

    public MqttClientOptions CreateOptions(
        MqttOptions options,
        MqttTlsCertificates certificates
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(certificates);

        X509Certificate2Collection trustChain = new()
        {
            certificates.CaCertificate
        };

        return new MqttClientOptionsBuilder()
            .WithClientId(options.ClientId)
            .WithTcpServer(options.Host, options.Port)
            .WithCredentials(options.Credentials.Username, options.Credentials.Password)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(options.KeepAliveSeconds))
            .WithTimeout(TimeSpan.FromSeconds(options.ConnectionTimeoutSeconds))
            .WithTlsOptions(tls =>
            {
                tls.UseTls();
                tls.WithTrustChain(trustChain);
                tls.WithClientCertificates( new[] { certificates.ClientCertificate });
                tls.WithSslProtocols(SslProtocols.Tls12);
            })
            .Build();
    }
}
