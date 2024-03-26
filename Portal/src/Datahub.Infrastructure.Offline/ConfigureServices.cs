using System.Globalization;
using System.Text;
using Askmethat.Aspnet.JsonLocalizer.Extensions;
using Blazored.LocalStorage;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Notebooks;
using Datahub.Application.Services.Security;
using Datahub.CatalogSearch;
using Datahub.Core.Services;
using Datahub.Core.Services.Achievements;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Services.Metadata;
using Datahub.Core.Services.UserManagement;
using Datahub.Infrastructure.Offline.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Infrastructure.Offline;

public static class ConfigureServices
{
        
    public static IServiceCollection AddDatahubOfflineInfrastructureServices(this IServiceCollection services, DatahubPortalConfiguration? portalConfiguration = null)
    {
        portalConfiguration ??= new DatahubPortalConfiguration();
        services.AddSingleton<CultureService>();
        services.AddDatahubLocalization(portalConfiguration);
        
        services.AddScoped<IMetadataBrokerService, MetadataBrokerService>();
        services.AddScoped<IDatahubAuditingService, OfflineDatahubTelemetryAuditingService>();
        services.AddSingleton<ICatalogSearchEngine, CatalogSearchEngine>();
        services.AddScoped<IAzurePriceListService, OfflineAzurePriceListService>();
        services.AddScoped<IKeyVaultUserService, OfflineKeyVaultUserService>();
        services.AddScoped<IPortalUserTelemetryService, OfflinePortalUserTelemetryService>();
        services.AddScoped<IUserInformationService, OfflineUserInformationService>();
        services.AddSingleton<IDatahubCatalogSearch, OfflineDatahubCatalogSearch>();
        services.AddScoped<IKeyVaultService, OfflineKeyVaultService>();
        services.AddScoped<IProjectUserManagementService, OfflineProjectUserManagementService>();
        services.AddScoped<IDatabricksApiService, OfflineDatabricksApiService>();
        
        services.AddBlazoredLocalStorage();
        
        
        return services;
    }

    public static IServiceCollection AddDatahubLocalization(this IServiceCollection services, DatahubPortalConfiguration portalConfiguration)
    {
        var supportedCultures = new HashSet<CultureInfo>
        {
            new("en-CA"),
            new("fr-CA")
        };

        services.AddJsonLocalization(options =>
        {
            options.CacheDuration = TimeSpan.FromMinutes(15);
            options.ResourcesPath = portalConfiguration.CultureSettings.ResourcesPath;
            options.AdditionalResourcePaths = portalConfiguration.CultureSettings.AdditionalResourcePaths;
            options.UseBaseName = false;
            options.IsAbsolutePath = true;
            options.LocalizationMode = Askmethat.Aspnet.JsonLocalizer.JsonOptions.LocalizationMode.I18n;
            options.MissingTranslationLogBehavior = MissingTranslationLogBehavior.Ignore;
            options.FileEncoding = Encoding.GetEncoding("UTF-8");
            options.SupportedCultureInfos = supportedCultures;
        });

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(supportedCultures.First());
            options.SupportedCultures = supportedCultures.ToList();
            options.SupportedUICultures = supportedCultures.ToList();
        });

        return services;
    }
}