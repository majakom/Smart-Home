
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartHome.Api.Tests.Integration;

public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SmartHome"] =
                        "Host=localhost;Port=5432;Database=smarthome_tests;Username=test;Password=test"
                }
            );
        });

        builder.ConfigureTestServices(ConfigureAdditionalServices);
    }

    protected virtual void ConfigureAdditionalServices(
        IServiceCollection services
    )
    {
    }
}
