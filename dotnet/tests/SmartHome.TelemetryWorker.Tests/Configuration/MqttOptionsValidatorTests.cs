
using Microsoft.Extensions.Options;
using SmartHome.TelemetryWorker.Configuration;

namespace SmartHome.TelemetryWorker.Tests.Configuration;

public class MqttOptionsValidatorTests
{
    private readonly MqttOptionsValidator _validator = new();

    [Fact]
    public void Validate_ReturnsSuccess_WhenOptionsAreValid()
    {
        // Arrange
        MqttOptions options = CreateValidOptions();

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Empty(result.Failures ?? []);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ReturnsFailure_WhenHostIsMissing(string? host)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.Host = host!;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT host is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void Validate_ReturnsFailure_WhenPortIsOutOfRange(int port)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.Port = port;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT port must be between 1 and 65535.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(65535)]
    public void Validate_ReturnsSuccess_WhenPortIsAtBoundary(int port)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.Port = port;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ReturnsFailure_WhenClientIdIsMissing(string? clientId)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.ClientId = clientId!;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT Client ID is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ReturnsFailure_WhenKeepAliveIsNotPositive(int seconds)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.KeepAliveSeconds = seconds;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT keep-alive must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ReturnsFailure_WhenConnectionTimeoutIsNotPositive(int seconds)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.ConnectionTimeoutSeconds = seconds;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT connection timeout must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ReturnsFailure_WhenReconnectDelayIsNotPositive(int seconds)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.ReconnectDelaySeconds = seconds;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT reconnect delay must be greater than zero.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ReturnsFailure_WhenUsernameIsMissing(string? username)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.Credentials.Username = username!;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT username is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ReturnsFailure_WhenPasswordIsMissing(string? password)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.Credentials.Password = password!;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT password is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ReturnsFailure_WhenCaCertificatePathIsMissing(string? path)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.Tls.CaCertificatePath = path!;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT CA certificate path is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ReturnsFailure_WhenClientCertificatePathIsMissing(string? path)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.Tls.ClientCertificatePath = path!;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT client certificate path is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ReturnsFailure_WhenClientKeyPathIsMissing(string? path)
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.Tls.ClientKeyPath = path!;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        AssertFailureContains(result, "MQTT client private key path is required.");
    }

    [Fact]
    public void Validate_ReturnsAllFailures_WhenMultiplePropertiesAreInvalid()
    {
        // Arrange
        MqttOptions options = CreateValidOptions();
        options.Host = string.Empty;
        options.Port = 0;
        options.Credentials.Username = string.Empty;
        options.Tls.CaCertificatePath = string.Empty;

        // Act
        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Collection(
            result.Failures!,
            failure => Assert.Equal("MQTT host is required.", failure),
            failure => Assert.Equal("MQTT port must be between 1 and 65535.", failure),
            failure => Assert.Equal("MQTT username is required.", failure),
            failure => Assert.Equal("MQTT CA certificate path is required.", failure)
        );
    }

    private static MqttOptions CreateValidOptions()
    {
        return new MqttOptions
        {
            Host = "localhost",
            Port = 8883,
            ClientId = "smarthome-test-client",
            KeepAliveSeconds = 30,
            ConnectionTimeoutSeconds = 10,
            ReconnectDelaySeconds = 5,
            Credentials = new CredentialsOptions
            {
                Username = "test-user",
                Password = "test-password"
            },
            Tls = new TlsOptions
            {
                CaCertificatePath = "ca.crt",
                ClientCertificatePath = "client.crt",
                ClientKeyPath = "client.key"
            }
        };
    }

    private static void AssertFailureContains(
        ValidateOptionsResult result,
        string expectedFailure)
    {
        Assert.False(result.Succeeded);
        Assert.Contains(expectedFailure, result.Failures!);
    }
}
