using Microsoft.Extensions.Options;

namespace SmartHome.TelemetryWorker.Configuration;

public sealed class MqttOptionsValidator : IValidateOptions<MqttOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        MqttOptions options
    )
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            errors.Add("MQTT host is required.");
        }

        if (options.Port is <= 0 or > 65535)
        {
            errors.Add("MQTT port must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            errors.Add("MQTT Client ID is required.");
        }

        if (options.KeepAliveSeconds <= 0)
        {
            errors.Add("MQTT keep-alive must be greater than zero.");
        }

        if (options.ConnectionTimeoutSeconds <= 0)
        {
            errors.Add("MQTT connection timeout must be greater than zero.");
        }

        if (options.ReconnectDelaySeconds <= 0)
        {
            errors.Add("MQTT reconnect delay must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(options.Credentials.Username))
        {
            errors.Add("MQTT username is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Credentials.Password))
        {
            errors.Add("MQTT password is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Tls.CaCertificatePath))
        {
            errors.Add("MQTT CA certificate path is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Tls.ClientCertificatePath))
        {
            errors.Add("MQTT client certificate path is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Tls.ClientKeyPath))
        {
            errors.Add("MQTT client private key path is required.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
