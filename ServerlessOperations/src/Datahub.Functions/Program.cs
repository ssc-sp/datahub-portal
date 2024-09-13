using Datahub.Core.Model.Datahub;
using Datahub.Functions;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Azure;
using Datahub.Infrastructure.Services.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Application.Services;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.Projects;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Storage;
using Datahub.Functions.Services;
using Datahub.Functions.Providers;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Services.Cost;
using Datahub.Infrastructure.Services.Security;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.Extensions.Azure;
using Datahub.Core.Model.Context;
using Datahub.Infrastructure;
using Datahub.Infrastructure.Services.Helpers;
using Datahub.Infrastructure.Services.ResourceGroups;
using Datahub.Shared.Configuration;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();
    })
    .ConfigureServices((hostContext, services) =>
    {
        var config = hostContext.Configuration;

        hostContext.HostingEnvironment.IsDevelopment();
        
        var connectionString = config["datahub_mssql_project"];
        if (connectionString is not null)
        {
            services.AddPooledDbContextFactory<DatahubProjectDBContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContextPool<DatahubProjectDBContext>(options => options.UseSqlServer(connectionString));
        }

        services.AddHttpClient(AzureManagementService.ClientName).AddPolicyHandler(
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(x => x.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(2), 5)));

        // TODO: implement this in a better way (unified between function and portal apps)
        var devopsConfig = config?.GetSection("AzureDevOpsConfiguration")?.Get<AzureDevOpsConfiguration>();
        if (devopsConfig != null)
        {
            services.AddSingleton(devopsConfig);
        }

        services.AddSingleton<AzureConfig>();
        services.AddSingleton<IAzureServicePrincipalConfig, AzureConfig>();
        services.AddSingleton<AzureManagementService>();
        services.AddAzureResourceManager(config);
        services.AddSingleton<IKeyVaultService, KeyVaultCoreService>();
        services.AddSingleton<IWorkspaceBudgetManagementService, WorkspaceBudgetManagementService>();
        services.AddSingleton<IWorkspaceCostManagementService, WorkspaceCostManagementService>();
        services.AddSingleton<IWorkspaceResourceGroupsManagementService, WorkspaceResourceGroupsManagementService>();
        services.AddSingleton<IWorkspaceStorageManagementService, WorkspaceStorageManagementService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<IAlertRecordService, AlertRecordService>();
        services.AddScoped<ProjectUsageService>();
        services.AddScoped<QueuePongService>();
        services.AddScoped<IResourceMessagingService, ResourceMessagingService>();
        services.AddScoped<IProjectInactivityNotificationService, ProjectInactivityNotificationService>();
        services.AddScoped<IProjectStorageConfigurationService, ProjectStorageConfigurationService>();
        services.AddScoped<IUserInactivityNotificationService, UserInactivityNotificationService>();
        services.AddScoped<IDateProvider, DateProvider>();
        services.AddScoped<EmailValidator>();
        services.AddScoped<HealthCheckHelper>();
        services.AddDatahubConfigurationFromFunctionFormat(config);
       

    })
    .Build();

host.Run();
