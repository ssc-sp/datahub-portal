using Datahub.Application.Services.Security;
using Datahub.Infrastructure.Services.Security;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddResourceProvisionerInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AzureDevOpsConfiguration>()
            .Bind(configuration.GetSection("InfrastructureRepository:AzureDevOpsConfiguration"))
            .ValidateDataAnnotations();

        // Expose the bound options as the concrete type and interface via factory,
        // so the value is resolved lazily after all configuration sources are loaded.
        services.AddSingleton<AzureDevOpsConfiguration>(sp =>
            sp.GetRequiredService<IOptions<AzureDevOpsConfiguration>>().Value);
        services.AddSingleton<IAzureDevopsConfiguration>(sp =>
            sp.GetRequiredService<AzureDevOpsConfiguration>());

        services.AddSingleton<AzAccessTokenManager>();
        services.AddSingleton<ISystemTokenCredentialService, InfraTokenCredentialService>();
        services.AddKeyedSingleton<ISystemTokenCredentialService, InfraTokenCredentialService>(SystemTokenCredentialServiceKeys.Infra);
        services.AddSingleton<IRepositoryService, RepositoryService>();
        services.AddHttpClient(RepositoryService.HttpClientName, async (sp, client) =>
        {
            var credentialService = sp.GetRequiredService<ISystemTokenCredentialService>();
            var tokenManager = sp.GetRequiredService<AzAccessTokenManager>();
            var accessToken = await tokenManager.AccessDevopsTokenAsync();
            client.DefaultRequestHeaders.Add("Authorization", $"{accessToken.TokenType} {accessToken.Token}");
        });
        services.AddSingleton<AzureDevOpsClient>();

        services.AddSingleton<ITerraformService, TerraformService>();

        return services;
    }
}
