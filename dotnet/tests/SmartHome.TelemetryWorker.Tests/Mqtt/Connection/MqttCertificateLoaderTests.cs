using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SmartHome.TelemetryWorker.Configuration;
using SmartHome.TelemetryWorker.Mqtt.Connection;

namespace SmartHome.TelemetryWorker.Tests.Mqtt.Connection;

public class MqttCertificateLoaderTests
{
    [Fact]
    public void Load_ThrowsArgumentNullException_WhenOptionsAreNull()
    {
        // Arrange
        using TemporaryDirectory directory = new();
        MqttCertificateLoader loader = CreateLoader(directory.Path);

        // Act
        Action act = () => loader.Load(null!);

        // Assert
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(act);

        Assert.Equal("options", exception.ParamName);
    }

    [Fact]
    public void Load_ThrowsFileNotFoundException_WhenCaCertificateDoesNotExist()
    {
        // Arrange
        using TemporaryDirectory directory = new();
        MqttCertificateLoader loader = CreateLoader(directory.Path);
        TlsOptions options = CreateOptions();

        // Act
        Action act = () => loader.Load(options);

        // Assert
        FileNotFoundException exception = Assert.Throws<FileNotFoundException>(act);
        Assert.Equal(
            Path.Combine(directory.Path, "ca.crt"),
            exception.FileName
        );
    }

    [Fact]
    public void Load_ThrowsFileNotFoundException_WhenClientCertificateDoesNotExist()
    {
        // Arrange
        using TemporaryDirectory directory = new();
        CreateCertificateFiles(directory.Path, createClientCertificate: false);

        MqttCertificateLoader loader = CreateLoader(directory.Path);
        TlsOptions options = CreateOptions();

        // Act
        Action act = () => loader.Load(options);

        // Assert
        FileNotFoundException exception = Assert.Throws<FileNotFoundException>(act);
        Assert.Equal(
            Path.Combine(directory.Path, "client.crt"),
            exception.FileName
        );
    }

    [Fact]
    public void Load_ThrowsFileNotFoundException_WhenClientPrivateKeyDoesNotExist()
    {
        // Arrange
        using TemporaryDirectory directory = new();
        CreateCertificateFiles(directory.Path, createClientKey: false);

        MqttCertificateLoader loader = CreateLoader(directory.Path);
        TlsOptions options = CreateOptions();

        // Act
        Action act = () => loader.Load(options);

        // Assert
        FileNotFoundException exception = Assert.Throws<FileNotFoundException>(act);
        Assert.Equal(
            Path.Combine(directory.Path, "client.key"),
            exception.FileName
        );
    }

    [Fact]
    public void Load_ThrowsCryptographicException_WhenCaCertificateIsInvalid()
    {
        // Arrange
        using TemporaryDirectory directory = new();
        CreateCertificateFiles(directory.Path);

        File.WriteAllText(
            Path.Combine(directory.Path, "ca.crt"),
            "invalid certificate"
        );

        MqttCertificateLoader loader = CreateLoader(directory.Path);
        TlsOptions options = CreateOptions();

        // Act
        Action act = () => loader.Load(options);

        // Assert
        Assert.Throws<CryptographicException>(act);
    }

    [Fact]
    public void Load_ThrowsCryptographicException_WhenClientCertificateIsInvalid()
    {
        // Arrange
        using TemporaryDirectory directory = new();
        CreateCertificateFiles(directory.Path);

        File.WriteAllText(
            Path.Combine(directory.Path, "client.crt"),
            "invalid certificate"
        );

        MqttCertificateLoader loader = CreateLoader(directory.Path);
        TlsOptions options = CreateOptions();

        // Act
        Action act = () => loader.Load(options);

        // Assert
        Assert.Throws<CryptographicException>(act);
    }

    [Fact]
    public void Load_ThrowsCryptographicException_WhenClientPrivateKeyIsInvalid()
    {
        // Arrange
        using TemporaryDirectory directory = new();
        CreateCertificateFiles(directory.Path);

        File.WriteAllText(
            Path.Combine(directory.Path, "client.key"),
            "invalid private key"
        );

        MqttCertificateLoader loader = CreateLoader(directory.Path);
        TlsOptions options = CreateOptions();

        // Act
        Action act = () => loader.Load(options);

        // Assert
        Assert.Throws<CryptographicException>(act);
    }

     [Fact]
    public void Load_ReturnsCertificates_WhenAllFilesAreValid()
    {
        // Arrange
        using TemporaryDirectory directory = new();
        CreateCertificateFiles(directory.Path);

        MqttCertificateLoader loader = CreateLoader(directory.Path);
        TlsOptions options = CreateOptions();

        // Act
        using MqttTlsCertificates result = loader.Load(options);

        // Assert
        Assert.Equal("CN=SmartHome Test CA", result.CaCertificate.Subject);
        Assert.Equal("CN=SmartHome Test Client", result.ClientCertificate.Subject);
        Assert.True(result.ClientCertificate.HasPrivateKey);
    }

    [Fact]
    public void Load_LoadsCertificates_WhenPathsAreAbsolute()
    {
        // Arrange
        using TemporaryDirectory directory = new();
        CreateCertificateFiles(directory.Path);

        MqttCertificateLoader loader = CreateLoader(directory.Path);

        TlsOptions options = new()
        {
            CaCertificatePath = Path.Combine(directory.Path, "ca.crt"),
            ClientCertificatePath = Path.Combine(directory.Path, "client.crt"),
            ClientKeyPath = Path.Combine(directory.Path, "client.key")
        };

        // Act
        using MqttTlsCertificates result = loader.Load(options);

        // Assert
        Assert.Equal("CN=SmartHome Test CA", result.CaCertificate.Subject);
        Assert.Equal("CN=SmartHome Test Client", result.ClientCertificate.Subject);
        Assert.True(result.ClientCertificate.HasPrivateKey);
    }

    private static MqttCertificateLoader CreateLoader(string contentRootPath)
    {
        TestHostEnvironment environment = new(contentRootPath);

        return new MqttCertificateLoader(environment);
    }

    private static TlsOptions CreateOptions()
    {
        return new TlsOptions
        {
            CaCertificatePath = "ca.crt",
            ClientCertificatePath = "client.crt",
            ClientKeyPath = "client.key"
        };
    }

    private static void CreateCertificateFiles(
        string directory,
        bool createClientCertificate = true,
        bool createClientKey = true)
    {
        Directory.CreateDirectory(directory);

        using RSA caKey = RSA.Create(2048);

        CertificateRequest caRequest = new(
            "CN=SmartHome Test CA",
            caKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        using X509Certificate2 caCertificate = caRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(30)
        );

        File.WriteAllText(
            Path.Combine(directory, "ca.crt"),
            caCertificate.ExportCertificatePem()
        );

        using RSA clientKey = RSA.Create(2048);

        CertificateRequest clientRequest = new(
            "CN=SmartHome Test Client",
            clientKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        using X509Certificate2 clientCertificate = clientRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(30)
        );

        if (createClientCertificate)
        {
            File.WriteAllText(
                Path.Combine(directory, "client.crt"),
                clientCertificate.ExportCertificatePem()
            );
        }

        if (createClientKey)
        {
            File.WriteAllText(
                Path.Combine(directory, "client.key"),
                clientKey.ExportPkcs8PrivateKeyPem()
            );
        }
    }

    private class TemporaryDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"SmartHome.Tests.{Guid.NewGuid():N}"
        );

        public TemporaryDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }

    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "SmartHome.TelemetryWorker.Tests";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public TestHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
        }
    }
}
