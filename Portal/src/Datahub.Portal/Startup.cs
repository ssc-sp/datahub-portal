using Askmethat.Aspnet.JsonLocalizer.Extensions;
using BlazorDownloadFile;
using Blazored.LocalStorage;
using Datahub.Application;
using Datahub.Application.Services;
using Datahub.CatalogSearch;
using Datahub.Core;
using Datahub.Core.Configuration;
using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Modules;
using Datahub.Core.Services;
using Datahub.Core.Services.Achievements;
using Datahub.Core.Services.Api;
using Datahub.Core.Services.Data;
using Datahub.Core.Services.Docs;
using Datahub.Core.Services.Metadata;
using Datahub.Core.Services.Notification;
using Datahub.Core.Services.Offline;
using Datahub.Core.Services.Projects;
using Datahub.Core.Services.Search;
using Datahub.Core.Services.Security;
using Datahub.Core.Services.Storage;
using Datahub.Core.Services.UserManagement;
using Datahub.Core.Services.Wiki;
using Datahub.Infrastructure;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Azure;
using Datahub.Infrastructure.Services.Projects;
using Datahub.Metadata.Model;
using Datahub.Portal.Middleware;
using Datahub.Portal.Services;
using Datahub.Portal.Services.Api;
using Datahub.Portal.Services.Auth;
using Datahub.Portal.Services.Notification;
using Datahub.Portal.Services.Offline;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor;
using MudBlazor.Services;
using Polly;
using Polly.Extensions.Http;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Infrastructure.Offline;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Application.Services.WebApp;
using Datahub.Infrastructure.Services.Api;
using Datahub.Infrastructure.Services.Metadata;
using Datahub.Infrastructure.Services.Notification;
using Tewr.Blazor.FileReader;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Configuration;
using Datahub.Infrastructure.Services.Security;
using Datahub.Application.Services.Publishing;
using Datahub.Infrastructure.Services.Publishing;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Infrastructure.Services.UserManagement;
using Datahub.Infrastructure.Services.ReverseProxy;
using Datahub.Infrastructure.Services.WebApp;
using Microsoft.Extensions.Azure;

[assembly: InternalsVisibleTo("Datahub.Tests")]

namespace Datahub.Portal;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        _currentEnvironment = env;
    }

    private readonly IConfiguration Configuration;
    private readonly IWebHostEnvironment _currentEnvironment;
    private ModuleManager moduleManager = new ModuleManager();

    private bool ResetDB => (bool)Configuration.GetSection("InitialSetup")?.GetValue("ResetDB", false);

    private bool EnsureDeleteinOffline =>
        (bool)Configuration.GetSection("InitialSetup")?.GetValue("EnsureDeleteinOffline", false);

    private bool Offline => Configuration.GetValue("Offline", false);

    private bool Debug => Configuration.GetValue("DebugMode", false);

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();

        services.AddDistributedMemoryCache();

        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
            options.HandleSameSiteCookieCompatibility();
        });

        //required to access existing headers
        services.AddHttpContextAccessor();
        services.AddOptions();

        // use this method to setup the authentication and authorization
        services.AddAuthenticationServices(Configuration);

        services.AddRazorPages()
            .AddMicrosoftIdentityUI();

        services.AddServerSideBlazor()
            .AddCircuitOptions(o =>
            {
                o.DetailedErrors = true; // todo: to make it 'true' only in development
            }).AddHubOptions(o =>
            {
                o.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
            });

        services.AddControllers();


        ConfigureLocalization(services);

        // add custom app services in this method
        ConfigureCoreDatahubServices(services);

        services.AddHttpClient();
        services.AddHttpClient<GraphServiceClient>()
            .AddPolicyHandler(GetRetryPolicy());
        services.AddFileReaderService();
        services.AddBlazorDownloadFile();
        services.AddBlazoredLocalStorage();
        services.AddScoped<ApiTelemetryService>();
        services.AddScoped<GetDimensionsService>();
        //TimeZoneService provides the user time zone to the server using JS Interop
        services.AddScoped<TimeZoneService>();

        services.AddUserAchievementServices();
        services.AddSecurityServices();

        services.AddElemental();
        services.AddMudServices();
        services.AddMudMarkdownServices();
        services.AddSingleton(moduleManager);


        moduleManager.LoadModules(Configuration.GetValue<string>("DataHubModules", "*"));
        foreach (var module in moduleManager.Modules)
        {
            Console.Write($"Configuring module {module.Name}\n");
            services.AddModule(module, Configuration);
        }

        // configure db contexts in this method
        ConfigureDbContexts(services);

        services.Configure<DataProjectsConfiguration>(Configuration.GetSection("DataProjectsConfiguration"));
        services.Configure<APITarget>(Configuration.GetSection("APITargets"));
        services.Configure<TelemetryConfiguration>(Configuration.GetSection("ApplicationInsights"));
        services.Configure<PortalVersion>(Configuration.GetSection("PortalVersion"));
        services.AddScoped<IPortalVersionService, PortalVersionService>();

        services.AddScoped<CatalogImportService>();
        services.AddSingleton<ICatalogSearchEngine, CatalogSearchEngine>();

        // TODO FIXME this will likely change when proper caching is implemented
        services.AddSingleton<IWikiService, WikiService>();
        services.AddSingleton<DocumentationService>();

        services.AddSingleton<CultureService>();

        services.AddSingleton<IAzureServicePrincipalConfig, AzureServicePrincipalConfig>();
        services.AddSingleton<AzureManagementService>();
        services.AddSingleton<ProjectUsageService>();
        services.AddScoped<ProjectStorageConfigurationService>();

        services.AddSignalRCore();

        var httpLoggingConfig = Configuration.GetSection("HttpLogging");
        var httpLoggingEnabled = httpLoggingConfig != null && httpLoggingConfig.GetValue<bool>("Enabled");

        if (httpLoggingEnabled)
        {
            var requestHeaders = httpLoggingConfig["RequestHeaders"]?.Split(",");
            var responseHeaders = httpLoggingConfig["ResponseHeaders"]?.Split(",");

            services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |
                                        HttpLoggingFields.ResponsePropertiesAndHeaders;

                if (requestHeaders is { Length: > 0 })
                {
                    foreach (var h in requestHeaders)
                    {
                        logging.RequestHeaders.Add(h);
                    }
                }

                if (responseHeaders is { Length: > 0 })
                {
                    foreach (var h in responseHeaders)
                    {
                        logging.ResponseHeaders.Add(h);
                    }
                }
            });
        }

        services.AddMiniProfiler().AddEntityFramework();

        if (ReverseProxyEnabled())
        {
            services.AddTelemetryConsumer<YarpTelemetryConsumer>();
            services.AddReverseProxy()
                    .AddTransforms(builderContext =>
                    {
                        builderContext.AddRequestTransform(async transformContext =>
                        {
                            // passing the logged user to the proxied app
                            var loggedUser = transformContext.HttpContext?.User?.Identity?.Name ?? "";
                            transformContext.ProxyRequest.Headers.Add(GetUserHeaderName(), loggedUser);
                            // add the role
                            transformContext.ProxyRequest.Headers.Add("role", transformContext.HttpContext.GetWorkspaceRole());
                            await Task.CompletedTask;
                        });
                    });            
        }
    }

    private bool ReverseProxyEnabled()
    {
        var datahubConfiguration = new DatahubPortalConfiguration();
        Configuration.Bind(datahubConfiguration);
        
        return datahubConfiguration.ReverseProxy.Enabled;
    }

    private string GetUserHeaderName() => Configuration.GetValue<string>("ReverseProxy:UserHeaderName") ?? "dh-user";

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private void InitializeDatabase<T>(ILogger logger, IDbContextFactory<T> dbContextFactory, bool migrate = true)
        where T : DbContext
    {
        EFTools.InitializeDatabase<T>(logger, Configuration, dbContextFactory, ResetDB, migrate,
            EnsureDeleteinOffline);
    }


    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger,
        IDbContextFactory<DatahubProjectDBContext> datahubFactory,
        IDbContextFactory<MetadataDbContext> metadataFactory)
    {
        if (Configuration.GetValue<bool>("HttpLogging:Enabled"))
        {
            app.UseHttpLogging();
        }

        foreach (var module in moduleManager.Modules)
        {
            logger.LogInformation($"Configuring module {module.Name}\n");
            app.ConfigureModule(module);
        }

        InitializeDatabase(logger, datahubFactory);
        InitializeDatabase(logger, metadataFactory, true);

        app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>()
            .Value);

        if (Debug)
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseMiniProfiler();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<WorkspaceAppMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapBlazorHub();
            endpoints.MapControllers();
            endpoints.MapFallbackToPage("/_Host");
            // reverse proxy
            var provider = endpoints.ServiceProvider.GetService<IProxyConfigProvider>();
            if (ReverseProxyEnabled() && provider != null)
            {
                endpoints.MapReverseProxy();
            }  
            else
            {
                logger.LogWarning($"Invalid Reverse Proxy configuration - No provider available");
            }
        });       
    }

    private void ConfigureLocalization(IServiceCollection services)
    {
        var cultureSection = Configuration.GetSection("CultureSettings");
        var trackTranslations = cultureSection.GetValue<bool>("TrackTranslations", false);
        var defaultCulture = cultureSection.GetValue<string>("Default","en-ca");
        var supportedCultures = cultureSection.GetValue<string>("SupportedCultures");
        var supportedCultureInfos = new HashSet<CultureInfo>(ParseCultures(supportedCultures));

        services.AddJsonLocalization(options =>
        {
            options.CacheDuration = TimeSpan.FromMinutes(15);
            options.ResourcesPath = "i18n";
            options.UseBaseName = false;
            options.IsAbsolutePath = true;
            options.LocalizationMode = Askmethat.Aspnet.JsonLocalizer.JsonOptions.LocalizationMode.I18n;
            options.MissingTranslationLogBehavior = trackTranslations
                ? MissingTranslationLogBehavior.CollectToJSON
                : MissingTranslationLogBehavior.Ignore;
            options.FileEncoding = Encoding.GetEncoding("UTF-8");
            options.SupportedCultureInfos = supportedCultureInfos;
        });

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
            options.SupportedCultures = supportedCultureInfos.ToList();
            options.SupportedUICultures = supportedCultureInfos.ToList();
        });
    }

    static IEnumerable<CultureInfo> ParseCultures(string values)
    {
        if (string.IsNullOrWhiteSpace(values))
            values = "en|fr";
        return (values ?? "").Split('|').Select(c => new CultureInfo($"{c[..2].ToLower()}-CA"));
    }

    private void ConfigureCoreDatahubServices(IServiceCollection services)
    {
        // configure online/offline services
        if (!Offline)
        {
            services.AddSingleton<IKeyVaultService, KeyVaultCoreService>();
            services.AddScoped<UserLocationManagerService>();
            services.AddSingleton<CommonAzureServices>();
            services.AddScoped<DataLakeClientService>();

            services.AddScoped<IUserInformationService, UserInformationService>();
            services.AddScoped<IUserSettingsService, UserSettingsService>();
            services.AddSingleton<IMSGraphService, MSGraphService>();

            services.AddScoped<IDataSharingService, DataSharingService>();
            services.AddScoped<IDataCreatorService, DataCreatorService>();
            services.AddScoped<DataRetrievalService>();
            services.AddScoped<IDataRemovalService, DataRemovalService>();

            services.AddScoped<IAzurePriceListService, AzurePriceListService>();
            services.AddScoped<IPublicDataFileService, PublicDataFileService>();

            services.AddScoped<UpdateProjectMonthlyCostService>();
            services.AddScoped<IProjectCreationService, ProjectCreationService>();

            services.AddScoped<IWorkspaceWebAppManagementService, WorkspaceWebAppManagementService>();
            
            services.AddDatahubApplicationServices(Configuration);
            services.AddDatahubInfrastructureServices(Configuration);

        }
        else
        {
            services.AddSingleton<IKeyVaultService, OfflineKeyVaultService>();
            services.AddScoped<UserLocationManagerService>();
            services.AddSingleton<CommonAzureServices>();
            //services.AddScoped<DataLakeClientService>();

            services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            services.AddScoped<IUserInformationService, OfflineUserInformationService>();
            services.AddScoped<IUserSettingsService, OfflineUserSettingsService>();
            services.AddSingleton<IMSGraphService, OfflineMSGraphService>();

            services.AddScoped<IDataSharingService, OfflineDataSharingService>();
            services.AddScoped<IDataCreatorService, OfflineDataCreatorService>();
            services.AddScoped<DataRetrievalService, OfflineDataRetrievalService>();
            services.AddScoped<IDataRemovalService, OfflineDataRemovalService>();
            services.AddScoped<IAzurePriceListService, OfflineAzurePriceListService>();
        }
        services.AddScoped<IProjectCreationService, ProjectCreationService>();


        services.AddScoped<IPublicDataFileService, PublicDataFileService>();

        services.AddSingleton<IExternalSearchService, ExternalSearchService>();
        services.AddHttpClient<IExternalSearchService, ExternalSearchService>();

        services.AddScoped<IMetadataBrokerService, MetadataBrokerService>();
        services.AddScoped<IDatahubAuditingService, DatahubTelemetryAuditingService>();
        services.AddScoped<IMiscStorageService, MiscStorageService>();

        services.AddScoped<DataImportingService>();
        services.AddSingleton<DatahubTools>();
        services.AddSingleton<TranslationService>();

        services.AddScoped<NotificationsService>();
        services.AddScoped<NotifierService>();

        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        services.AddScoped<PortalEmailService>();
        services.AddScoped<ISystemNotificationService, SystemNotificationService>();
        services.AddSingleton<IPropagationService, PropagationService>();

        services.AddSingleton<ServiceAuthManager>();

        services.AddSingleton<IOpenDataService, OpenDataService>();
        
        services.AddScoped<ITbsOpenDataService, TbsOpenDataService>();
        services.AddScoped<IOpenDataPublishingService, OpenDataPublishingService>();

        services.AddSingleton<IGlobalSessionManager, GlobalSessionManager>();
        services.AddScoped<IUserCircuitCounterService, UserCircuitCounterService>();

        services.AddScoped<IRequestManagementService, RequestManagementService>();

        services.AddScoped<CustomNavigation>();

        services.AddScoped<IOrganizationLevelsService, OrganizationLevelsService>();
    }

    private void ConfigureDbContexts(IServiceCollection services)
    {
        var projectsDatabaseConnectionString = Configuration.GetConnectionString("datahub_mssql_project");
        var useSqlite = projectsDatabaseConnectionString?.StartsWith("Data Source=") ?? false;
        
        ConfigureDbContext<DatahubProjectDBContext>(services, "datahub_mssql_project", useSqlite ? DbDriver.Sqlite : DbDriver.Azure);
        ConfigureDbContext<MetadataDbContext>(services, "datahub_mssql_metadata", DbDriver.Azure);
    }

    private void ConfigureDbContext<T>(IServiceCollection services, string connectionStringName, DbDriver dbDriver)
        where T : DbContext
    {
        services.ConfigureDbContext<T>(Configuration, connectionStringName, dbDriver);
    }
}
