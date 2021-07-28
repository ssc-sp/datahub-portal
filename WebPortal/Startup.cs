using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NRCan.Datahub.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;
using Tewr.Blazor.FileReader;
using BlazorDownloadFile;
using NRCan.Datahub.Portal.Services.Offline;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NRCan.Datahub.Shared.Services;
using Askmethat.Aspnet.JsonLocalizer.Extensions;
using System.Text;
using NRCan.Datahub.Shared.Data;
using Microsoft.EntityFrameworkCore;
using NRCan.Datahub.Data.Projects;
using NRCan.Datahub.ProjectForms.Data.PIP;
using NRCan.Datahub.Portal.EFCore;
using NRCan.Datahub.Shared.EFCore;
using NRCan.Datahub.Portal.Data;
using NRCan.Datahub.Portal.Data.Finance;
using NRCan.Datahub.Portal.Data.WebAnalytics;
using BlazorApplicationInsights;
using NRCan.Datahub.Portal.RoleManagement;
using NRCan.Datahub.Metadata;
using Microsoft.Graph;
using Polly;
using System.Net.Http;
using Polly.Extensions.Http;
using NRCan.Datahub.Metadata;
using Microsoft.Extensions.Logging;

namespace NRCan.Datahub.Portal
{

    public enum DbDriver
    {
        SqlServer, Sqlite
    }
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _currentEnvironment = env;
        }

        private readonly IConfiguration Configuration;
        private readonly IWebHostEnvironment _currentEnvironment;

        private bool Offline => _currentEnvironment.IsEnvironment("Offline");

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddBlazorApplicationInsights();

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
                o.DetailedErrors = true;
            }).AddHubOptions(o =>
            {
                o.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
            }).AddMicrosoftIdentityConsentHandler();

            services.AddControllers();

            ConfigureLocalization(services);

            // add custom app services in this method
            ConfigureDatahubServices(services);

            services.AddHttpClient();
            services.AddHttpClient<IGraphServiceClient, GraphServiceClient>().AddPolicyHandler(GetRetryPolicy());
            services.AddFileReaderService();
            services.AddBlazorDownloadFile();
            services.AddScoped<ApiTelemetryService>();
            services.AddScoped<GetDimensionsService>();
            services.AddElemental();

            // configure db contexts in this method
            ConfigureDbContexts(services);

            IConfigurationSection sec = Configuration.GetSection("APITargets");
            services.Configure<APITarget>(sec);

            services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();

            services.AddSignalRCore();
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                            retryAttempt)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger,
            IDbContextFactory<DatahubProjectDBContext> datahubFactory,
            IDbContextFactory<EFCoreDatahubContext> cosmosFactory,
            IDbContextFactory<FinanceDBContext> financeFactory,
            IDbContextFactory<PIPDBContext> pipFactory,
            IDbContextFactory<MetadataDbContext> metadataFactory,
            IDbContextFactory<DatahubETLStatusContext> etlFactory)
        {


            InitializeDatabase(logger, cosmosFactory, false);
            InitializeDatabase(logger, datahubFactory);
            InitializeDatabase(logger, etlFactory);
            InitializeDatabase(logger, financeFactory);
            InitializeDatabase(logger, pipFactory);
            InitializeDatabase(logger, metadataFactory);

            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

            if (env.IsDevelopment())
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

        private void InitializeDatabase<T>(ILogger<Startup> logger, IDbContextFactory<T> factory, bool migrate = true) where T:DbContext
        {
            using var context = factory.CreateDbContext();
            try
            {
                if (Offline)
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();                    
                }
                else
                {
                    context.Database.EnsureCreated();
                    if (migrate)
                        context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Error initializing database {context.Database.GetDbConnection().Database}-{typeof(T).Name}");
            }
            
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

                services.AddControllersWithViews(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));

                }).AddMicrosoftIdentityUI();
            }
        }

        private void ConfigureLocalization(IServiceCollection services)
        {
            var cultureSection = Configuration.GetSection("CultureSettings");

            var defaultCulture = cultureSection.GetValue<string>("Default");
            var supportedCultures = cultureSection.GetValue<string>("SupportedCultures");
            var supportedCultureInfos = new HashSet<CultureInfo>(ParseCultures(supportedCultures));

            services.AddJsonLocalization(options =>
            {
                options.CacheDuration = TimeSpan.FromMinutes(15);
                options.ResourcesPath = "i18n";
                options.UseBaseName = false;
                options.IsAbsolutePath = true;
                options.LocalizationMode = Askmethat.Aspnet.JsonLocalizer.JsonOptions.LocalizationMode.I18n;
                options.MissingTranslationLogBehavior = MissingTranslationLogBehavior.Ignore;
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
            return (values ?? "").Split('|').Select(c => new CultureInfo($"{c.Substring(0, 2).ToLower()}-CA"));
        }

        private void ConfigureDatahubServices(IServiceCollection services)
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

                services.AddScoped<IApiService, ApiService>();
                services.AddScoped<IApiCallService, ApiCallService>();

                services.AddScoped<IDataUpdatingService, DataUpdatingService>();
                services.AddScoped<IDataSharingService, DataSharingService>();
                services.AddScoped<IDataCreatorService, DataCreatorService>();
                services.AddScoped<IDataRetrievalService, DataRetrievalService>();
                services.AddScoped<IDataRemovalService, DataRemovalService>();

                services.AddSingleton<ICognitiveSearchService, CognitiveSearchService>();

                services.AddScoped<PowerBiServiceApi>();
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

                services.AddScoped<IApiService, OfflineApiService>();
                services.AddScoped<IApiCallService, OfflineApiCallService>();

                services.AddScoped<IDataUpdatingService, OfflineDataUpdatingService>();
                services.AddScoped<IDataSharingService, OfflineDataSharingService>();
                services.AddScoped<IDataCreatorService, OfflineDataCreatorService>();
                services.AddScoped<IDataRetrievalService, OfflineDataRetrievalService>();
                services.AddScoped<IDataRemovalService, OfflineDataRemovalService>();

                services.AddSingleton<ICognitiveSearchService, OfflineCognitiveSearchService>();
            }

            services.AddSingleton<IExternalSearchService, ExternalSearchService>();
            services.AddHttpClient<IExternalSearchService, ExternalSearchService>();

            services.AddScoped<IMetadataBrokerService, MetadataBrokerService>();

            services.AddScoped<DataImportingService>();
            services.AddSingleton<DatahubTools>();

            services.AddScoped<NotificationsService>();
            services.AddScoped<UIControlsService>();
            services.AddScoped<NotifierService>();

            services.AddSingleton<ServiceAuthManager>();

            services.AddSingleton<IExternalSearchService, ExternalSearchService>();
            services.AddHttpClient<IExternalSearchService, ExternalSearchService>();
        }

        private void ConfigureDbContexts(IServiceCollection services)
        {
            var driver = Offline? DbDriver.Sqlite: DbDriver.SqlServer;
            ConfigureDbContext<DatahubProjectDBContext>(services, "datahub-mssql-project", driver);
            ConfigureDbContext<PIPDBContext>(services, "datahub-mssql-pip", driver);
            ConfigureDbContext<FinanceDBContext>(services, "datahub-mssql-finance", driver);
            if (!Offline)
            {
                ConfigureCosmosDbContext<EFCoreDatahubContext>(services, "datahub-cosmosdb", "datahub-catalog-db");
            }
            else
            {
                ConfigureDbContext<EFCoreDatahubContext>(services, "datahub-cosmosdb", driver);
            }
            ConfigureDbContext<WebAnalyticsContext>(services, "datahub-mssql-webanalytics", driver);
            ConfigureDbContext<DatahubETLStatusContext>(services, "datahub-mssql-etldb", driver);
            ConfigureDbContext<MetadataDbContext>(services, "datahub-mssql-metadata", driver);
        }

        private void ConfigureDbContext<T>(IServiceCollection services, string connectionStringName, DbDriver dbDriver) where T : DbContext
        {
            var connectionString = GetConnectionString(connectionStringName);
            switch (dbDriver)
            {
                case DbDriver.SqlServer:
                    services.AddPooledDbContextFactory<T>(options => options.UseSqlServer(connectionString));
                    services.AddDbContextPool<T>(options => options.UseSqlServer(connectionString));
                    break;
                case DbDriver.Sqlite:
                    services.AddPooledDbContextFactory<T>(options => options.UseSqlite(connectionString));
                    services.AddDbContextPool<T>(options => options.UseSqlite(connectionString));
                    break;
            }
        }

        private void ConfigureCosmosDbContext<T>(IServiceCollection services, string connectionStringName, string catalogName) where T : DbContext
        {
            var connectionString = GetConnectionString(connectionStringName);
            services.AddPooledDbContextFactory<T>(options => options.UseCosmos(connectionString, databaseName: catalogName));
            services.AddDbContextPool<T>(options => options.UseCosmos(connectionString, databaseName: catalogName));
        }

        private string GetConnectionString(string name)
        {
            return Configuration.GetConnectionString(name) ?? throw new ArgumentNullException($"ASPNETCORE_CONNECTION STRING ({name})");
        }
    }
}
