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

namespace NRCan.Datahub.Portal
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _currentEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _currentEnvironment;

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

            if (!_currentEnvironment.IsEnvironment("Offline"))
            {
                services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                        .AddAzureAD(options => Configuration.Bind("AzureAd", options));

                services.AddControllersWithViews(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));

                }).AddMicrosoftIdentityUI();
            }

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
            });

            services.AddControllers();

            var cultureSection = Configuration.GetSection("CultureSettings");

            var supportedCultures = new HashSet<CultureInfo>();
            var values = cultureSection.GetValue<string>("SupportedCultures");
            
            values.Split('|').ToList().ForEach(value => supportedCultures.Add(new CultureInfo($"{value.Substring(0, 2).ToLower()}-CA")));

            services.AddJsonLocalization(options => {
                options.CacheDuration = TimeSpan.FromMinutes(15);
                options.ResourcesPath = "i18n";
                options.UseBaseName = false;
                options.IsAbsolutePath = true;
                options.LocalizationMode = Askmethat.Aspnet.JsonLocalizer.JsonOptions.LocalizationMode.I18n;
                options.MissingTranslationLogBehavior = MissingTranslationLogBehavior.Ignore;
                options.FileEncoding = Encoding.GetEncoding("UTF-8");
                options.SupportedCultureInfos = supportedCultures;
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(cultureSection.GetValue<string>("Default"));
                options.SupportedCultures = supportedCultures.ToList();
                options.SupportedUICultures = supportedCultures.ToList();
            });

            //services.AddMvc()
            //    .AddViewLocalization(opts => { opts.ResourcesPath = "Resources"; })
            //    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            //    .AddDataAnnotationsLocalization();


            //services.Configure<AzureADOptions>(options => Configuration.Bind("AzureAd", options));
            //services.Configure<ConfidentialClientApplicationOptions>(options => Configuration.Bind("AzureAd", options));

            if (_currentEnvironment.IsEnvironment("Offline"))
            {
                services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
                services.AddScoped<IUserInformationService, OfflineUserInformationService>();
                services.AddSingleton<IMSGraphService, OfflineMSGraphService>();
                services.AddSingleton<IKeyVaultService, OfflineKeyVaultService>();
                services.AddScoped<IApiService, OfflineApiService>();
            }
            else
            {
                services.AddScoped<IUserInformationService, UserInformationService>();
                services.AddSingleton<IMSGraphService, MSGraphService>();
                services.AddSingleton<IKeyVaultService, KeyVaultService>();
                services.AddScoped<IApiService, ApiService>();
            }
            services.AddSingleton<CommonAzureServices>();
            services.AddScoped<ApiCallService>();
            services.AddScoped<DataUpdatingService>();
            services.AddScoped<DataSharingService>();
            services.AddScoped<DataCreatorService>();
            services.AddScoped<DataRetrievalService>();
            services.AddScoped<DataRemovalService>();
            services.AddScoped<DataImportingService>();
            services.AddSingleton<DatahubTools>();
            services.AddSingleton<CognitiveSearchService>();
            services.AddScoped<NotificationsService>();
            services.AddScoped<UIControlsService>();
            //services.AddScoped<Elemental.Services.UIControlsService>();
            services.AddHttpClient();
            services.AddFileReaderService();
            services.AddBlazorDownloadFile();
            services.AddScoped<NotifierService>();
            services.AddScoped<DataLakeClientService>();
            services.AddScoped<ApiTelemetryService>();
            services.AddScoped<GetDimensionsService>();
            services.AddElemental();

            services.AddDbContextFactory<DatahubProjectDBContext>(opt =>
                opt.UseSqlServer(Configuration.GetConnectionString("datahub-mssql-project") ?? throw new ArgumentNullException("ASPNETCORE_CONNECTION STRING")));
            services.AddDbContextPool<DatahubProjectDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("datahub-mssql-project") ?? throw new ArgumentNullException("ASPNETCORE_CONNECTION STRING")));
            services.AddDbContextPool<PIPDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("datahub-mssql-pip") ?? throw new ArgumentNullException("ASPNETCORE_CONNECTION STRING")));
            services.AddDbContextPool<FinanceDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("datahub-mssql-finance") ?? throw new ArgumentNullException("ASPNETCORE_CONNECTION STRING")));
            services.AddDbContextPool<EFCoreDatahubContext>(options =>
                options.UseCosmos(Configuration.GetConnectionString("datahub-cosmosdb") ?? throw new ArgumentNullException("ASPNETCORE_CONNECTION STRING"), databaseName: "datahub-catalog-db"));
            services.AddDbContextPool<WebAnalyticsContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("datahub-mssql-webanalytics") ?? throw new ArgumentNullException("ASPNETCORE_CONNECTION STRING")));

            services.AddDbContext<SqlCiosbDatahubEtldbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("datahub-mssql-etldb") ?? throw new ArgumentNullException("ASPNETCORE_CONNECTION STRING")));

            IConfigurationSection sec = Configuration.GetSection("APITargets");
            services.Configure<APITarget>(sec);

            services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();
            services.AddSingleton<ServiceAuthManager>();

            services.AddSignalRCore();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseRequestLocalization
                (app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

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


    }
}
