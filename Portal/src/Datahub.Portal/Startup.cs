using AspNetCore.Localizer.Json.Commons;
using AspNetCore.Localizer.Json.Extensions;
using AspNetCore.Localizer.Json.JsonOptions;
using BlazorDownloadFile;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Datahub.Application;
using Datahub.Application.Authentication;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.Metadata;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Publishing;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Application.Services.WebApp;
using Datahub.CatalogSearch;
using Datahub.Core.Configuration;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Services;
using Datahub.Core.Services.Api;
using Datahub.Core.Services.Data;
using Datahub.Core.Services.Docs;
using Datahub.Core.Services.Notification;
using Datahub.Core.Services.Offline;
using Datahub.Core.Services.Projects;
using Datahub.Core.Services.Search;
using Datahub.Core.Services.Storage;
using Datahub.Core.Model;
using Datahub.Core.Services.UserManagement;
using Datahub.Infrastructure;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Achievements;
using Datahub.Infrastructure.Services.Api;
using Datahub.Infrastructure.Services.Azure;
using Datahub.Infrastructure.Services.Metadata;
using Datahub.Infrastructure.Services.Notification;
using Datahub.Infrastructure.Services.Projects;
using Datahub.Infrastructure.Services.Publishing;
using Datahub.Infrastructure.Services.Security;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Infrastructure.Services.UserManagement;
using Datahub.Infrastructure.Services.WebApp;
using Datahub.Metadata.Model;
using Datahub.Portal.Controllers;
using Datahub.Portal.Pages;
using Datahub.Portal.Services;
using Datahub.Portal.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
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
using Tewr.Blazor.FileReader;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;

[assembly: InternalsVisibleTo("Datahub.Tests")]

namespace Datahub.Portal;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private readonly IConfiguration Configuration;
    private readonly IWebHostEnvironment _currentEnvironment;

    private bool ResetDB => (Configuration.GetSection("InitialSetup")?.GetValue("ResetDB", false))??false;

    private bool EnsureDeleteinOffline =>
        (Configuration.GetSection("InitialSetup")?.GetValue("EnsureDeleteinOffline", false))??false;

    private bool Offline => Configuration.GetValue("Offline", false);

    private bool Debug => Configuration.GetValue("DebugMode", false);

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        // To consume scoped services, AddScopedFeatureManagement should be used instead of AddFeatureManagement.
        // This will ensure that feature management services, including feature filters, targeting context accessor, are added as scoped services.
        services.AddScopedFeatureManagement();
        services.AddApplicationInsightsTelemetry(x =>
        {
            x.ConnectionString = Configuration["ApplicationInsights:ConnectionString"];
        });

        services.AddDistributedMemoryCache();

        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
            // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
            options.HandleSameSiteCookieCompatibility();
        });

        services.AddSession(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Name = ".FSDH.Session";
            options.IdleTimeout = TimeSpan.FromMinutes(30);
        });

        //required to access existing headers
        services.AddHttpContextAccessor();
        services.AddOptions();

        // use this method to setup the authentication and authorization
        services.AddAuthenticationServices(Configuration);
        services.AddAuthorization();

        services.AddRazorPages()        
            .AddMicrosoftIdentityUI();

        services.AddRazorComponents()
            .AddInteractiveServerComponents()
                        .AddCircuitOptions(o =>
                        {
                            o.DetailedErrors = false;
                        }).AddHubOptions(o =>
                        {
                            o.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
                        });

        //services.AddServerSideBlazor()
        //    .AddCircuitOptions(o =>
        //    {
        //        o.DetailedErrors = true; // todo: to make it 'true' only in development
        //    }).AddHubOptions(o =>
        //    {
        //        o.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
        //    });

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
        services.AddBlazoredSessionStorage();
        services.AddHttpContextAccessor();
        services.AddScoped<ApiTelemetryService>();

        services.AddUserAchievementServices();
        services.AddSecurityServices();

        services.AddMudServices();
        services.AddMudMarkdownServices();

        // configure db contexts in this method
        ConfigureDbContexts(services);

        services.Configure<APITargets>(Configuration.GetSection(nameof(APITargets)));
        services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(Configuration.GetSection("ApplicationInsights"));
        services.Configure<PortalVersion>(Configuration.GetSection("PortalVersion"));

        // Diagnostic dump (redacted) using shared Core utility
        ConfigurationHelper.DumpRedactedToConsole("API targets", Configuration.GetSection(nameof(APITargets)));
        ConfigurationHelper.DumpRedactedToConsole("Application Insights", Configuration.GetSection("ApplicationInsights"));

        services.AddScoped<IPortalVersionService, PortalVersionService>();

        services.AddScoped<CatalogImportService>();
        services.AddSingleton<ICatalogSearchEngine, CatalogSearchEngine>();

        // TODO FIXME this will likely change when proper caching is implemented
        services.AddSingleton<DocumentationService>();

        services.AddScoped<ICultureService, UserCultureService>();

        services.AddSingleton<IAzureServicePrincipalConfig, AzureServicePrincipalConfig>();
        services.AddScoped<ProjectStorageConfigurationService>();

        //https://github.com/jsakamoto/Toolbelt.Blazor.LocalTimeText/
        services.AddLocalTimeZoneServer();

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

        // Configure DevAuth only when DevAuth:UserEmail is specified
        var devAuthSection = Configuration.GetSection("GccfOidc:DevAuth");
        var devAuthUserEmail = devAuthSection.GetValue<string>("UserEmail");
        if (!string.IsNullOrWhiteSpace(devAuthUserEmail))
        {
            services.Configure<DevAuthOptions>(devAuthSection);
            services.AddScoped<DevAuthDBEntities>();
        }
    }

    private bool ReverseProxyEnabled()
    {
        var datahubConfiguration = new DatahubPortalConfiguration();
        Configuration.Bind(datahubConfiguration);
        
        return datahubConfiguration.ReverseProxy.Enabled;
    }  

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
    IConfiguration configuration,
    IDbContextFactory<MetadataDbContext> metadataFactory)
    {
        var services = app.ApplicationServices;

        if (Configuration.GetValue<bool>("HttpLogging:Enabled"))
        {
            app.UseHttpLogging();
        }
        var dbDriver = configuration.GetDriver();
        if (dbDriver == DbDriver.Sqlite)
        {
            var ctx = services.GetRequiredService<IDbContextFactory<SqliteDatahubContext>>();
            InitializeDatabase(logger, ctx);
        }
        else
        {
            var ctx = services.GetRequiredService<IDbContextFactory<SqlServerDatahubContext>>();
            InitializeDatabase(logger, ctx);
        }

        InitializeDatabase(logger, metadataFactory, true);

        app.UseRequestLocalization(services.GetService<IOptions<RequestLocalizationOptions>>()
            .Value);

        if (Debug)
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseMiniProfiler();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseStatusCodePagesWithReExecute("/404");

        // this needs to be as late as possible in the pipeline to ensure antiforgery tokens are validated for all endpoints, including reverse proxy
        app.UseAntiforgery();

        app.UseMiddleware<IFrameMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            var provider = services.GetService<IProxyConfigProvider>();
            if (ReverseProxyEnabled() && provider != null)
            {
                endpoints.MapReverseProxy();
            }
            else
            {
                logger.LogWarning($"Invalid Reverse Proxy configuration - No provider available");
            }

            endpoints.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        });

        // Run DevAuth bootstrap only when dev auth is configured
        var devAuthSection = configuration.GetSection("GccfOidc:DevAuth");
        var devAuthUserEmail = devAuthSection.GetValue<string>("UserEmail");
        if (env.IsDevelopment() && !string.IsNullOrWhiteSpace(devAuthUserEmail))
        {
            using var scope = services.CreateScope();
            var bootstrapper = scope.ServiceProvider.GetRequiredService<DevAuthDBEntities>();
            bootstrapper.EnsureDevUserAsync().GetAwaiter().GetResult();
        }
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
            options.LocalizationMode = LocalizationMode.I18n;
            options.UseEmbeddedResources = true;
            options.MissingTranslationLogBehavior = trackTranslations
                ? MissingTranslationLogBehavior.CollectToJSON
                : MissingTranslationLogBehavior.Ignore;
            options.FileEncoding = Encoding.GetEncoding("UTF-8");
            options.SupportedCultureInfos = supportedCultureInfos;
            options.AssemblyHelper = new AssemblyHelper(typeof(Startup).Assembly);
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
            services.AddSingleton<IKeyVaultCoreService, KeyVaultCoreService>();
            services.AddScoped<UserLocationManagerService>();
            services.AddSingleton<CommonAzureServices>();
            services.AddScoped<DataLakeClientService>();

            services.AddScoped<IUserInformationService, UserInformationService>();
            services.AddScoped<IUserSettingsService, UserSettingsService>();
            services.AddSingleton<IMSGraphService, MSGraphService>();

            services.AddScoped<IAzurePriceListService, AzurePriceListService>();

            services.AddScoped<UpdateProjectMonthlyCostService>();
            services.AddScoped<IWorkspaceCreationService, WorkspaceCreationService>();
            services.AddScoped<IProjectDeletionService, ProjectDeletionService>();
            services.AddScoped<IOrganizationLevelsService, OrganizationLevelsService>();

            services.AddScoped<IWorkspaceWebAppManagementService, WorkspaceWebAppManagementService>();
            
            services.AddDatahubApplicationServices(Configuration);
            services.AddDatahubInfrastructureServices(Configuration);

        }
        else
        {
            services.AddSingleton<IKeyVaultCoreService, OfflineKeyVaultService>();
            services.AddScoped<UserLocationManagerService>();
            services.AddSingleton<CommonAzureServices>();

            services.AddScoped<AuthenticationStateProvider, FakeAuthStateProvider>();
            services.AddScoped<IUserInformationService, OfflineUserInformationService>();
            services.AddScoped<IUserSettingsService, OfflineUserSettingsService>();
            services.AddSingleton<IMSGraphService, OfflineMSGraphService>();

            services.AddScoped<IAzurePriceListService, OfflineAzurePriceListService>();

            services.AddScoped<IWorkspaceCostManagementService, OfflineWorkspaceCostManagementService>();
            
            
        }
        services.AddScoped<IWorkspaceCreationService, WorkspaceCreationService>();
        services.AddScoped<IWorkspaceVersionService, WorkspaceVersionService>();

        services.AddSingleton<IExternalSearchService, ExternalSearchService>();
        services.AddHttpClient<IExternalSearchService, ExternalSearchService>();

        services.AddScoped<IMetadataBrokerService, MetadataBrokerService>();
        services.AddScoped<IDatahubAuditingService, DatahubTelemetryAuditingService>();
        services.AddScoped<IMiscStorageService, MiscStorageService>();

        services.AddScoped<DataImportingService>();
        services.AddSingleton<DatahubTools>();
        services.AddSingleton<TranslationService>();

        services.AddScoped<NotificationsService>();

        services.AddScoped<IGCNotifyService, GCNotifyService>();
        services.AddScoped<IUserAccessNotificationService, UserAccessNotificationService>();
        services.AddScoped<ISystemNotificationService, SystemNotificationService>();
        services.AddSingleton<IPropagationService, NotificationPropagationService>();

        services.AddSingleton<IOpenDataService, OpenDataService>();
        
        services.AddScoped<ITbsOpenDataService, TbsOpenDataService>();
        services.AddScoped<IOpenDataPublishingService, OpenDataPublishingService>();

        services.AddSingleton<IGlobalSessionManager, GlobalSessionManager>();
        services.AddScoped<IUserCircuitCounterService, UserCircuitCounterService>();

        services.AddScoped<IRequestManagementService, RequestManagementService>();

        services.AddScoped<CustomNavigation>();

        services.AddScoped<IDownloadService, DownloadService>();
        services.AddScoped<ICsvService, CsvService>();
        
        services.AddScoped<IFileScanService, FileScanService>();

        services.AddTransient<CorrelationIdHandler>();
        services.AddHttpClient<ExternalSearchService>()
            .AddHttpMessageHandler<CorrelationIdHandler>();
    }

    private void ConfigureDbContexts(IServiceCollection services)
    {
        var projectsDatabaseConnectionString = Configuration.GetConnectionString("datahub_mssql_project");
        var useSqlite = projectsDatabaseConnectionString?.StartsWith("Data Source=") ?? false;
        
        ConfigureDbContext<DatahubProjectDBContext, SqlServerDatahubContext,SqliteDatahubContext>(services, "datahub_mssql_project", useSqlite ? DbDriver.Sqlite : DbDriver.Azure);
        ConfigureDbContext<MetadataDbContext, SqlServerMetadataDbContext, SqlServerMetadataDbContext>(services, "datahub_mssql_metadata", DbDriver.Azure);
    }

    private void ConfigureDbContext<TGen, Tsql, Tsqlite>(IServiceCollection services, string connectionStringName, DbDriver dbDriver)
        where TGen : DbContext where Tsql : DbContext where Tsqlite : DbContext
    {
        services.ConfigureDbContext<TGen, Tsql, Tsqlite>(Configuration, connectionStringName, dbDriver);
    }
    public class CorrelationIdHandler : DelegatingHandler
    {
        private readonly ISessionStorageService _sessionStorage;

        public CorrelationIdHandler(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = await _sessionStorage.GetItemAsStringAsync("correlationId");

            if (!string.IsNullOrEmpty(correlationId))
            {
                request.Headers.Add("X-Correlation-ID", correlationId);
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
