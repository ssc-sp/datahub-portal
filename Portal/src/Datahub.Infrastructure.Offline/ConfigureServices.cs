using System.Globalization;
using System.Text;
using Askmethat.Aspnet.JsonLocalizer.Extensions;
using Blazored.LocalStorage;
using Datahub.CatalogSearch;
using Datahub.Core.RoleManagement;
using Datahub.Core.Services;
using Datahub.Core.Services.Achievements;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Services.Metadata;
using Datahub.Core.Services.Notification;
using Datahub.Core.Services.Offline;
using Datahub.Core.Services.Security;
using Datahub.Core.Services.UserManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Infrastructure.Offline;

public static class ConfigureServices
{
        
    public static IServiceCollection AddDatahubOfflineInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        
        services.AddSingleton<CultureService>();
        services.AddDatahubLocalization();
        
        services.AddScoped<IMetadataBrokerService, MetadataBrokerService>();
        services.AddScoped<IDatahubAuditingService, OfflineDatahubTelemetryAuditingService>();
        services.AddSingleton<ICatalogSearchEngine, CatalogSearchEngine>();
        services.AddScoped<IAzurePriceListService, OfflineAzurePriceListService>();
        services.AddScoped<IPortalUserTelemetryService, OfflinePortalUserTelemetryService>();
        services.AddScoped<IUserInformationService, OfflineUserInformationService>();
        services.AddSingleton<IDatahubCatalogSearch, OfflineDatahubCatalogSearch>();
        services.AddScoped<IKeyVaultService, OfflineKeyVaultService>();
        
        
        
        services.AddBlazoredLocalStorage();
        
        
        return services;
    }

    public static IServiceCollection AddDatahubLocalization(this IServiceCollection services)
    {
        var supportedCultures = new HashSet<CultureInfo>
        {
            new("en-CA"),
            new("fr-CA")
        };

        services.AddJsonLocalization(options =>
        {
            options.CacheDuration = TimeSpan.FromMinutes(15);
            options.ResourcesPath = "../Datahub.Portal/i18n";
            options.AdditionalResourcePaths = new[] { "../Datahub.Portal/i18n/ssc" };
            options.UseBaseName = false;
            options.IsAbsolutePath = true;
            options.LocalizationMode = Askmethat.Aspnet.JsonLocalizer.JsonOptions.LocalizationMode.I18n;
            options.MissingTranslationLogBehavior = MissingTranslationLogBehavior.CollectToJSON;
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