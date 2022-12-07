using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

using ResourceProvisioner.Application.Services;
using Microsoft.AspNetCore.Hosting;

namespace ResourceProvisioner.Application.IntegrationTests;

using static Testing;
internal class CustomWebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var integrationConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
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