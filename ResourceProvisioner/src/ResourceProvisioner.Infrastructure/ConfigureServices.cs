using Datahub.Application.Services.Security;
using Datahub.Infrastructure.Services.Security;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Services;
using System.Net.Http.Headers;

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
        services.AddHttpClient(RepositoryService.HttpClientName, (sp, client) =>
        {
            try
            {
                var tokenManager = sp.GetRequiredService<AzAccessTokenManager>();
                var accessToken = tokenManager.AccessDevopsTokenAsync().GetAwaiter().GetResult();

                if (string.IsNullOrWhiteSpace(accessToken.TokenType) || string.IsNullOrWhiteSpace(accessToken.Token))
                {
                    throw new InvalidOperationException("Azure DevOps access token is missing token type or token value.");
                }

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(accessToken.TokenType, accessToken.Token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to configure Azure DevOps HttpClient authorization header.", ex);
            }
        });
        services.AddSingleton<AzureDevOpsClient>();

        services.AddSingleton<ITerraformService, TerraformService>();

        return services;
    }
}
