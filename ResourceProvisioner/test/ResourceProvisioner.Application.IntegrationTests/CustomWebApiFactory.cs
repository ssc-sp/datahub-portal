using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace ResourceProvisioner.Application.IntegrationTests;

internal class CustomWebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var integrationConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            configurationBuilder.AddConfiguration(integrationConfig);
        });

        builder.ConfigureServices((builder, services) =>
        {
            // services.Remove<IUserAccessorService>()
            //     .AddScoped(_ => Mock.Of<IUserAccessorService>(x => x.GetUserId() == GetCurrentUserId()));
        });
    }
}