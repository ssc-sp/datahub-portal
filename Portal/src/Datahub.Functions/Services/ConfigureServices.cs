using Datahub.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Functions.Services;

public static class ConfigureServices
{
        public static IServiceCollection AddDatahubConfigurationFromFunctionFormat(this IServiceCollection services, IConfiguration configuration)
        {
            var datahubConfiguration = new DatahubPortalConfiguration();
            configuration.Bind(datahubConfiguration);
            
            datahubConfiguration.DatahubStorageQueue.ConnectionString = configuration["DatahubStorageConnectionString"] ?? throw new ArgumentNullException("DatahubStorageConnectionString");
            
            datahubConfiguration.AzureAd.ClientId = configuration["FUNC_SP_CLIENT_ID"] ?? throw new ArgumentNullException("FUNC_SP_CLIENT_ID");
            
            datahubConfiguration.AzureAd.ClientSecret = configuration["FUNC_SP_CLIENT_SECRET"] ?? throw new ArgumentNullException("FUNC_SP_CLIENT_SECRET");
            
            services.AddSingleton(datahubConfiguration);

            return services;
        }
}