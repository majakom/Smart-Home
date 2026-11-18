namespace SmartHome.TelemetryWorker.Configuration;

public sealed class MqttOptions
{
    public const string SectionName = "Mqtt";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 8883;
    public string ClientId { get; set; } = "smarthome-telemetry-worker";
    public int KeepAliveSeconds { get; set; } = 30;
    public int ConnectionTimeoutSeconds { get; set; } = 10;
    public int ReconnectDelaySeconds { get; set; } = 5;
    public CredentialsOptions Credentials { get; set; } = new();
    public TlsOptions Tls { get; set; } = new();
}

public sealed class CredentialsOptions
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

}

public sealed class TlsOptions
{
    public string CaCertificatePath { get; set; } = string.Empty;

    public string ClientCertificatePath { get; set; } = string.Empty;

    public string ClientKeyPath { get; set; } = string.Empty;
}
