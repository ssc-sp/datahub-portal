using System.Net;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Application.Services;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Projects;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Storage;
using Datahub.Application.Services.UserManagement;
using Datahub.Application.Services.WebApp;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Functions;
using Datahub.Functions.Providers;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure;
using Datahub.Infrastructure.Offline.Security;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Azure;
using Datahub.Infrastructure.Services.Cost;
using Datahub.Infrastructure.Services.Helpers;
using Datahub.Infrastructure.Services.Notification;
using Datahub.Infrastructure.Services.Projects;
using Datahub.Infrastructure.Services.ResourceGroups;
using Datahub.Infrastructure.Services.Security;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Infrastructure.Services.WebApp;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Caching.Memory; // ADDED
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Contrib.WaitAndRetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

var config = builder.Configuration;
var env = builder.Environment;

var connectionString = config["datahub_mssql_project"];
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddPooledDbContextFactory<DatahubProjectDBContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddDbContextPool<DatahubProjectDBContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddHttpClient(IMSGraphService.HttpClientName)
    .AddPolicyHandler(
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(2), 5))
    );

var devopsConfig = config.GetSection("AzureDevOpsConfiguration").Get<AzureDevOpsConfiguration>();
if (devopsConfig is not null)
{
    builder.Services.AddSingleton(devopsConfig);
}

builder.Services.AddSingleton<AzureConfig>();
builder.Services.AddSingleton<IAzureServicePrincipalConfig, AzureConfig>();
builder.Services.AddAzureResourceManager(config);
builder.Services.AddSingleton<IKeyVaultService, KeyVaultCoreService>();
builder.Services.AddSingleton<IWorkspaceBudgetManagementService, WorkspaceBudgetManagementService>();
builder.Services.AddSingleton<IWorkspaceCostManagementService, WorkspaceCostManagementService>();
builder.Services.AddSingleton<IWorkspaceResourceGroupsManagementService, WorkspaceResourceGroupsManagementService>();
builder.Services.AddSingleton<IWorkspaceStorageManagementService, WorkspaceStorageManagementService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<IGCNotifyService, GCNotifyService>();
builder.Services.AddSingleton<IAlertRecordService, AlertRecordService>();
builder.Services.AddScoped<IQueuePongService, QueuePongService>();
builder.Services.AddScoped<IResourceMessagingService, ResourceMessagingService>();
builder.Services.AddScoped<IProjectInactivityNotificationService, ProjectInactivityNotificationService>();
builder.Services.AddScoped<IProjectStorageConfigurationService, ProjectStorageConfigurationService>();
builder.Services.AddScoped<IWorkspaceWebAppManagementService, WorkspaceWebAppManagementService>();
builder.Services.AddScoped<IUserInactivityNotificationService, UserInactivityNotificationService>();
builder.Services.AddScoped<IWorkspaceVersionService, WorkspaceVersionService>();
builder.Services.AddScoped<IDateProvider, DateProvider>();
builder.Services.AddScoped<EmailValidator>();
builder.Services.AddScoped<HealthCheckHelper>();
builder.Services.AddDatahubConfigurationFromFunctionFormat(config);
builder.Services.AddScoped<IKeyVaultUserService, OfflineKeyVaultUserService>();

// in-memory cache for health result
builder.Services.AddMemoryCache();

var host = builder.Build();
await host.RunAsync();
