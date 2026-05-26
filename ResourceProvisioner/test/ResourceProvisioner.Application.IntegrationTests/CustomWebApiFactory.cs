using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ResourceProvisioner.Application.IntegrationTests;

internal class CustomWebApiFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("test");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.SetBasePath(AppContext.BaseDirectory);
            configurationBuilder.AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.AddUserSecrets<Program>(optional: true);
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(AppContext.BaseDirectory);

        builder.ConfigureServices((builder, services) =>
        {
            // services.Remove<IUserAccessorService>()
            //     .AddScoped(_ => Mock.Of<IUserAccessorService>(x => x.GetUserId() == GetCurrentUserId()));
        });
    }
}
