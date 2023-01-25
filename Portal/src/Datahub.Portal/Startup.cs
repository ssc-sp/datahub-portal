using Askmethat.Aspnet.JsonLocalizer.Extensions;
using BlazorDownloadFile;
using Blazored.LocalStorage;
using Datahub.Achievements;
using Datahub.Achievements.Models;
using Datahub.CatalogSearch;
using Datahub.CKAN.Service;
using Datahub.Core;
using Datahub.Core.Configuration;
using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.UserTracking;
using Datahub.Core.Modules;
using Datahub.Core.Services;
using Datahub.Core.Services.Api;
using Datahub.Core.Services.AzureCosting;
using Datahub.Core.Services.Data;
using Datahub.Core.Services.Metadata;
using Datahub.Core.Services.Notification;
using Datahub.Core.Services.Offline;
using Datahub.Core.Services.Projects;
using Datahub.Core.Services.Search;
using Datahub.Core.Services.Security;
using Datahub.Core.Services.Storage;
using Datahub.Core.Services.UserManagement;
using Datahub.Core.Services.Wiki;
using Datahub.GeoCore.Service;
using Datahub.Metadata.Model;
using Datahub.PIP.Data;
using Datahub.Portal.Data.Forms.WebAnalytics;
using Datahub.Portal.Data.Pipelines;
using Datahub.Portal.Services;
using Datahub.Portal.Services.Api;
using Datahub.Portal.Services.Offline;
using Datahub.PowerBI.Services;
using Datahub.PowerBI.Services.Offline;
using Datahub.ProjectTools.Services;
using Datahub.ProjectTools.Services.Offline;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using MudBlazor;
using MudBlazor.Services;
using Polly;
using Polly.Extensions.Http;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Datahub.Application;
using Datahub.Application.Services;
using Datahub.Portal.Services.Auth;
using Microsoft.Identity.Web.UI;
using Tewr.Blazor.FileReader;
using Datahub.Core.Services.ResourceManager;
using Datahub.Core.Services.Docs;
using Datahub.Infrastructure;
using Datahub.Infrastructure.Services;
using Datahub.Portal.Services.Notification;
using Datahub.LanguageTraining.Services;
using Datahub.M365Forms.Services;

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

    private bool ResetDB => ((bool)Configuration.GetSection("InitialSetup")?.GetValue("ResetDB", false));

    private bool EnsureDeleteinOffline =>
        ((bool)Configuration.GetSection("InitialSetup")?.GetValue("EnsureDeleteinOffline", false));

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
        services.AddAchievementService(opts =>
        {
            opts.Enabled = Configuration.GetValue("Achievements:Enabled", false);
            opts.AchievementDirectoryPath = Path.Join(AppContext.BaseDirectory, "Achievements");
        });

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
        services.Configure<CKANConfiguration>(Configuration.GetSection("CKAN"));
        services.Configure<GeoCoreConfiguration>(Configuration.GetSection("GeoCore"));
        services.Configure<PortalVersion>(Configuration.GetSection("PortalVersion"));
        services.AddScoped<IPortalVersionService, PortalVersionService>();
        services.AddProjectResources();

        services.AddScoped<CatalogImportService>();
        services.AddSingleton<ICatalogSearchEngine, CatalogSearchEngine>();

        // TODO FIXME this will likely change when proper caching is implemented
        services.AddSingleton<IWikiService, WikiService>();
        services.AddSingleton<DocumentationService>();

        services.AddSingleton<CultureService>();

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
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                retryAttempt)));
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
        IDbContextFactory<UserTrackingContext> userTrackingFactory,
        IDbContextFactory<AchievementContext> achievementFactory,
        IDbContextFactory<PIPDBContext> pipFactory,
        IDbContextFactory<MetadataDbContext> metadataFactory,
        IDbContextFactory<DatahubETLStatusContext> etlFactory)
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
        InitializeDatabase(logger, userTrackingFactory, false);
        InitializeDatabase(logger, achievementFactory, false);
        InitializeDatabase(logger, etlFactory);
        InitializeDatabase(logger, pipFactory);
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
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapBlazorHub();
            endpoints.MapControllers();
            endpoints.MapFallbackToPage("/_Host");
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
            options.AdditionalResourcePaths = new[] { $"i18n/{Program.GetDataHubProfile()}" };
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
            services.AddSingleton<IKeyVaultService, KeyVaultService>();
            services.AddScoped<UserLocationManagerService>();
            services.AddSingleton<CommonAzureServices>();
            services.AddScoped<DataLakeClientService>();

            services.AddScoped<IUserInformationService, UserInformationService>();
            services.AddSingleton<IMSGraphService, MSGraphService>();

            services.AddScoped<IProjectDatabaseService, ProjectDatabaseService>();

            services.AddScoped<IDataSharingService, DataSharingService>();
            services.AddScoped<IDataCreatorService, DataCreatorService>();
            services.AddScoped<DataRetrievalService>();
            services.AddScoped<IDataRemovalService, DataRemovalService>();

            services.AddScoped<IAzurePriceListService, AzurePriceListService>();
            services.AddScoped<IPublicDataFileService, PublicDataFileService>();

            services.AddScoped<PowerBiServiceApi>();
            services.AddScoped<IPowerBiDataService, PowerBiDataService>();

            services.AddScoped<UpdateProjectMonthlyCostService>();
            services.AddScoped<IProjectCreationService, ProjectCreationService>();
            services.AddDatahubApplicationServices();
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
            services.AddSingleton<IMSGraphService, OfflineMSGraphService>();
            services.AddScoped<IPowerBiDataService, OfflinePowerBiDataService>();

            services.AddScoped<IProjectDatabaseService, OfflineProjectDatabaseService>();

            services.AddScoped<IDataSharingService, OfflineDataSharingService>();
            services.AddScoped<IDataCreatorService, OfflineDataCreatorService>();
            services.AddScoped<DataRetrievalService, OfflineDataRetrievalService>();
            services.AddScoped<IDataRemovalService, OfflineDataRemovalService>();
            services.AddScoped<IAzurePriceListService, OfflineAzurePriceListService>();
        }
        services.AddScoped<IProjectCreationService, ProjectCreationService>();
        services.AddSingleton<RequestQueueService>();


        services.AddScoped<IPublicDataFileService, PublicDataFileService>();

        services.AddSingleton<IExternalSearchService, ExternalSearchService>();
        services.AddHttpClient<IExternalSearchService, ExternalSearchService>();

        services.AddScoped<IMetadataBrokerService, MetadataBrokerService>();
        services.AddScoped<IDatahubAuditingService, DatahubTelemetryAuditingService>();
        services.AddScoped<IMiscStorageService, MiscStorageService>();

        services.AddScoped<DataImportingService>();
        services.AddSingleton<DatahubTools>();
        services.AddSingleton<TranslationService>();
        services.AddSingleton<GitHubToolsService>();

        services.AddScoped<NotificationsService>();
        services.AddScoped<UIControlsService>();
        services.AddScoped<NotifierService>();
        services.AddScoped<AzureCostManagementService>();

        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        services.AddScoped<PortalEmailService>();
        services.AddScoped<ProjectToolsEmailService>();
        services.AddScoped<LanguageEmailService>();
        services.AddScoped<PowerBiEmailService>();
        services.AddScoped<M365EmailService>();
        services.AddScoped<ISystemNotificationService, SystemNotificationService>();
        services.AddSingleton<IPropagationService, PropagationService>();

        services.AddSingleton<ServiceAuthManager>();

        services.AddCKANService();
        services.AddSingleton<IOpenDataService, OpenDataService>();

        services.AddGeoCoreService();

        services.AddSingleton<IGlobalSessionManager, GlobalSessionManager>();
        services.AddScoped<IUserCircuitCounterService, UserCircuitCounterService>();

        services.AddScoped<IRequestManagementService, RequestManagementService>();

        services.AddScoped<CustomNavigation>();

        services.AddScoped<IOrganizationLevelsService, OrganizationLevelsService>();
    }

    private void ConfigureDbContexts(IServiceCollection services)
    {
        ConfigureDbContext<DatahubProjectDBContext>(services, "datahub-mssql-project", Configuration.GetDriver());
        ConfigureDbContext<PIPDBContext>(services, "datahub-mssql-pip", Configuration.GetDriver());
        if (Configuration.GetDriver() == DbDriver.Azure)
        {
            ConfigureCosmosDbContext<UserTrackingContext>(services, "datahub-cosmosdb", "datahub-catalog-db");
            ConfigureCosmosDbContext<AchievementContext>(services, "datahub-cosmosdb", "datahub-catalog-db");
        }
        else
        {
            ConfigureDbContext<UserTrackingContext>(services, "datahub-cosmosdb", Configuration.GetDriver());
            ConfigureDbContext<AchievementContext>(services, "datahub-cosmosdb", Configuration.GetDriver());
        }

        ConfigureDbContext<WebAnalyticsContext>(services, "datahub-mssql-webanalytics", Configuration.GetDriver());
        ConfigureDbContext<DatahubETLStatusContext>(services, "datahub-mssql-etldb", Configuration.GetDriver());
        ConfigureDbContext<MetadataDbContext>(services, "datahub-mssql-metadata", Configuration.GetDriver());
    }

    private void ConfigureDbContext<T>(IServiceCollection services, string connectionStringName, DbDriver dbDriver)
        where T : DbContext
    {
        services.ConfigureDbContext<T>(Configuration, connectionStringName, dbDriver);
    }

    private void ConfigureCosmosDbContext<T>(IServiceCollection services, string connectionStringName,
        string catalogName) where T : DbContext
    {
        var connectionString = Configuration.GetConnectionString(_currentEnvironment, connectionStringName);
        services.AddPooledDbContextFactory<T>(options =>
            options.UseCosmos(connectionString, databaseName: catalogName));
        services.AddDbContextPool<T>(options => options.UseCosmos(connectionString, databaseName: catalogName));
    }
}