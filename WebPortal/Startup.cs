using System.Globalization;
using Datahub.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Tewr.Blazor.FileReader;
using BlazorDownloadFile;
using Datahub.Portal.Services.Offline;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Datahub.Core.Services;
using Askmethat.Aspnet.JsonLocalizer.Extensions;
using System.Text;
using Datahub.Core.Data;
using Microsoft.EntityFrameworkCore;
using Datahub.Portal.Data.PIP;
using Datahub.Core.EFCore;
using Datahub.Portal.Data;
using Datahub.Portal.Data.WebAnalytics;
using Microsoft.Graph;
using Polly;
using Polly.Extensions.Http;
using Datahub.Metadata.Model;
using Datahub.Core.RoleManagement;
using Datahub.Core;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpLogging;
using Datahub.CKAN.Service;
using Datahub.Core.UserTracking;
using System.Runtime.CompilerServices;
using Blazored.LocalStorage;
using Datahub.Achievements;
using Datahub.Core.Configuration;
using Datahub.Core.Modules;
using Datahub.Portal.Services.Storage;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MudBlazor.Services;
using Datahub.GeoCore.Service;
using MudBlazor.Services;
using Datahub.Core.Services.Offline;

[assembly: InternalsVisibleTo("Datahub.Tests")]

namespace Datahub.Portal
{
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

        private bool ResetDB => (Configuration.GetSection("InitialSetup")?.GetValue<bool>("ResetDB", false) ?? false);
        private bool EnsureDeleteinOffline => (Configuration.GetSection("InitialSetup")?.GetValue<bool>("EnsureDeleteinOffline", false) ?? false);
        private bool Offline => Configuration.GetValue<bool>("Offline", false);

        private bool Debug => Configuration.GetValue<bool>("DebugMode", false);

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
            ConfigureAuthentication(services);

            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
            });

            services.AddRazorPages();
            services.AddServerSideBlazor().AddCircuitOptions(o =>
            {
                o.DetailedErrors = true; // todo: to make it 'true' only in development
            }).AddHubOptions(o =>
            {
                o.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
            }).AddMicrosoftIdentityConsentHandler();

            services.AddControllers();

            ConfigureLocalization(services);

            // add custom app services in this method
            ConfigureCoreDatahubServices(services);

            services.AddHttpClient();
            services.AddHttpClient<GraphServiceClient>().AddPolicyHandler(GetRetryPolicy());
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
            services.AddSingleton(moduleManager);


            moduleManager.LoadModules(Configuration.GetValue<string>("DataHubModules", "*"));
            foreach (var module in moduleManager.Modules)
            {
                Console.Write($"Configuring module {module.Name}\n");
                services.AddModule(module,Configuration);
            }

            // configure db contexts in this method
            ConfigureDbContexts(services);

            services.Configure<DataProjectsConfiguration>(Configuration.GetSection("ProjectTools"));

            services.Configure<APITarget>(Configuration.GetSection("APITargets"));

            services.Configure<TelemetryConfiguration>(Configuration.GetSection("ApplicationInsights"));

            services.Configure<CKANConfiguration>(Configuration.GetSection("CKAN"));
            services.Configure<GeoCoreConfiguration>(Configuration.GetSection("GeoCore"));

            services.Configure<SessionsConfig>(Configuration.GetSection("Sessions"));

            services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();

            services.Configure<PortalVersion>(Configuration.GetSection("PortalVersion"));
            services.AddScoped<IPortalVersionService, PortalVersionService>();

            services.AddScoped<CatalogImportService>();

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

                    if (requestHeaders != null && requestHeaders.Length > 0)
                    {
                        foreach (var h in requestHeaders)
                        {
                            logging.RequestHeaders.Add(h);
                        }
                    }

                    if (responseHeaders != null && responseHeaders.Length > 0)
                    {
                        foreach (var h in responseHeaders)
                        {
                            logging.ResponseHeaders.Add(h);
                        }
                    }
                });
            }
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)));
        }

        private void InitializeDatabase<T>(ILogger logger, IDbContextFactory<T> dbContextFactory, bool migrate = true) where T : DbContext
        {
            EFTools.InitializeDatabase<T>(logger, Configuration, dbContextFactory, ResetDB, migrate,
                EnsureDeleteinOffline);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger,
            IDbContextFactory<DatahubProjectDBContext> datahubFactory,
            IDbContextFactory<UserTrackingContext> userTrackingFactory,
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
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            //https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-call-api-app-configuration?tabs=aspnetcore
            //services.AddSignIn(Configuration, "AzureAd")
            //        .AddInMemoryTokenCaches();

            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            // JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Token acquisition service based on MSAL.NET
            // and chosen token cache implementation

            if (!Offline)
            {
                //services.AddAuthentication(AzureADDefaults.AuthenticationScheme)               
                //        .AddAzureAD(options => Configuration.Bind("AzureAd", options));
                var scopes = new List<string>();
                //scopes.AddRange(PowerBiServiceApi.RequiredReadScopes);
                scopes.Add("user.read");
                //scopes.Add("PowerBI.Read.All");

                services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApp(Configuration, "AzureAd")
                    .EnableTokenAcquisitionToCallDownstreamApi(scopes)
                    .AddMicrosoftGraph(Configuration.GetSection("Graph"))
                    .AddInMemoryTokenCaches();

                services.AddControllersWithViews()
                    .AddMicrosoftIdentityUI();
            }
        }

        private void ConfigureLocalization(IServiceCollection services)
        {
            var cultureSection = Configuration.GetSection("CultureSettings");
            var trackTranslations = cultureSection.GetValue<bool>("TrackTranslations", false);
            var defaultCulture = cultureSection.GetValue<string>("Default");
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

                services.AddScoped<IMyDataService, MyDataService>();

                services.AddScoped<IPublicDataFileService, PublicDataFileService>();

                services.AddScoped<IProjectDatabaseService, ProjectDatabaseService>();

                services.AddScoped<IDataUpdatingService, DataUpdatingService>();
                services.AddScoped<IDataSharingService, DataSharingService>();
                services.AddScoped<IDataCreatorService, DataCreatorService>();
                services.AddScoped<DataRetrievalService>();
                services.AddScoped<IDataRemovalService, DataRemovalService>();

                services.AddSingleton<ICognitiveSearchService, CognitiveSearchService>();

                services.AddScoped<IAzurePriceListService, AzurePriceListService>();

                services.AddScoped<PowerBiServiceApi>();
                services.AddScoped<IPowerBiDataService, PowerBiDataService>();

                services.AddScoped<RegistrationService>();
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

                services.AddScoped<IMyDataService, OfflineMyDataService>();

                services.AddScoped<IProjectDatabaseService, OfflineProjectDatabaseService>();

                services.AddScoped<IDataUpdatingService, OfflineDataUpdatingService>();
                services.AddScoped<IDataSharingService, OfflineDataSharingService>();
                services.AddScoped<IDataCreatorService, OfflineDataCreatorService>();
                services.AddScoped<DataRetrievalService, OfflineDataRetrievalService>();
                services.AddScoped<IDataRemovalService, OfflineDataRemovalService>();

                services.AddScoped<IAzurePriceListService, OfflineAzurePriceListService>();

                services.AddSingleton<ICognitiveSearchService, OfflineCognitiveSearchService>();
            }

            services.AddSingleton<IExternalSearchService, ExternalSearchService>();
            services.AddHttpClient<IExternalSearchService, ExternalSearchService>();

            services.AddScoped<IMetadataBrokerService, MetadataBrokerService>();
            services.AddScoped<IDatahubAuditingService, DatahubTelemetryAuditingService>();
            services.AddScoped<IMiscStorageService, MiscStorageService>();

            services.AddScoped<DataImportingService>();
            services.AddSingleton<DatahubTools>();
            services.AddSingleton<TranslationService>();

            services.AddScoped<NotificationsService>();
            services.AddScoped<UIControlsService>();
            services.AddScoped<NotifierService>();

            services.AddScoped<IEmailNotificationService, EmailNotificationService>();
            services.AddScoped<ISystemNotificationService, SystemNotificationService>();
            services.AddSingleton<IPropagationService, PropagationService>();

            services.AddSingleton<ServiceAuthManager>();

            services.AddCKANService();
            services.AddSingleton<IOpenDataService, OpenDataService>();

            services.AddGeoCoreService();

            services.AddSingleton<IGlobalSessionManager, GlobalSessionManager>();
            services.AddScoped<IUserCircuitCounterService, UserCircuitCounterService>();

            services.AddScoped<RequestManagementService>();

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
            }
            else
            {
                ConfigureDbContext<UserTrackingContext>(services, "datahub-cosmosdb", Configuration.GetDriver());
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
}