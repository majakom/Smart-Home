using System.Security.Cryptography.X509Certificates;

namespace SmartHome.TelemetryWorker.Mqtt.Connection;

public sealed class MqttTlsCertificates(
    X509Certificate2 caCertificate,
    X509Certificate2 clientCertificate
) : IDisposable
{
    public X509Certificate2 CaCertificate { get; } = caCertificate;
    public X509Certificate2 ClientCertificate { get; } = clientCertificate;

    public void Dispose()
    {
        CaCertificate.Dispose();
        ClientCertificate.Dispose();
    }
}
