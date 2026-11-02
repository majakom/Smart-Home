using DotNetEnv;
using SmartHome.Application;
using SmartHome.Infrastructure;
using SmartHome.TelemetryWorker;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Env.NoClobber().TraversePath().Load();

        var builder = Host.CreateApplicationBuilder(args);
        
        string connectionString =
            builder.Configuration.GetConnectionString("SmartHome")
            ?? throw new InvalidOperationException(
                "Connection string 'SmartHome' is not configured."
            );

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(connectionString);
        builder.Services.AddTelemetryWorker(builder.Configuration);

        IHost host = builder.Build();
        await host.RunAsync();
    }
}
