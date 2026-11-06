using System.Security.Cryptography.X509Certificates;
using SmartHome.TelemetryWorker.Configuration;

namespace SmartHome.TelemetryWorker.Mqtt.Connection;

public sealed class MqttCertificateLoader(IHostEnvironment environment)
{
    public MqttTlsCertificates Load(TlsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        X509Certificate2? caCertificate = null;
        X509Certificate2? clientCertificate = null;

        try
        {
            caCertificate = LoadCertificate(options.CaCertificatePath);

            clientCertificate = LoadClientCertificate(
                options.ClientCertificatePath,
                options.ClientKeyPath
            );

            return new MqttTlsCertificates(caCertificate, clientCertificate);
        }
        catch
        {
            caCertificate?.Dispose();
            clientCertificate?.Dispose();

            throw;
        }
    }

    private X509Certificate2 LoadCertificate(string certificatePath)
    {
        string resolvedCertificatePath = ResolvePath(certificatePath);
        return X509CertificateLoader.LoadCertificateFromFile(resolvedCertificatePath);
    }

    private X509Certificate2 LoadClientCertificate(string certificatePath, string privateKeyPath)
    {
        string resolvedCertificatePath = ResolvePath(certificatePath);
        string resolvedPrivateKeyPath = ResolvePath(privateKeyPath);
        return X509Certificate2.CreateFromPemFile(resolvedCertificatePath, resolvedPrivateKeyPath);
    }

    private string ResolvePath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            throw new InvalidOperationException("An MQTT certificate path is not configured.");

        string path = Path.GetFullPath(configuredPath, environment.ContentRootPath);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Configured MQTT certificate file was not found: '{path}'.", path);

        return path;
    }
}
