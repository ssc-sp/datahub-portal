using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace ResourceProvisioner.Application.IntegrationTests;

internal class CustomWebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(AppContext.BaseDirectory);
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var integrationConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            configurationBuilder.AddConfiguration(integrationConfig);
            // add secrets to config
            configurationBuilder.AddUserSecrets<CustomWebApiFactory>();
        });

        builder.ConfigureServices((builder, services) =>
        {
            // services.Remove<IUserAccessorService>()
            //     .AddScoped(_ => Mock.Of<IUserAccessorService>(x => x.GetUserId() == GetCurrentUserId()));
        });
    }
}
