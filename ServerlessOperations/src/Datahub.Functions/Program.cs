using Datahub.Application.Services.Cost;
using Datahub.Application.Services.UserManagement;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Functions;
using Datahub.Functions.Services;
using Datahub.Infrastructure;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Cost;
using Datahub.Infrastructure.Services.ResourceGroups;
using Datahub.Infrastructure.Services.Security;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;
using Datahub.Core.Configuration;

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

builder.Services.AddDatahubConfigurationFromFunctionFormat(config);

builder.Services.AddSingleton<AzureConfig>();
builder.Services.AddSingleton<IAzureConfiguration, AzureConfig>();
builder.Services.AddSingleton<IAlertRecordService, AlertRecordService>();
builder.Services.AddSingleton<ISystemTokenCredentialService, SystemTokenCredentialService>();
builder.Services.AddKeyedSingleton<ISystemTokenCredentialService, SystemTokenCredentialService>(SystemTokenCredentialServiceKeys.Infra);
builder.Services.AddAzureResourceManager(config);
builder.Services.AddSingleton<IKeyVaultCoreService, KeyVaultCoreService>();
builder.Services.AddSingleton<IWorkspaceBudgetManagementService, WorkspaceBudgetManagementService>();
builder.Services.AddSingleton<IWorkspaceCostManagementService, WorkspaceCostManagementService>();
builder.Services.AddSingleton<IWorkspaceResourceGroupsManagementService, WorkspaceResourceGroupsManagementService>();
builder.Services.AddSingleton<IWorkspaceStorageManagementService, WorkspaceStorageManagementService>();
// IServiceBusConfiguration is only required to create workspace definitions shouldn't be required here
builder.Services.AddSingleton<IServiceBusConfiguration, NoServiceBusConfiguration>();

// in-memory cache for health result
builder.Services.AddMemoryCache();

builder.Services.AddFunctionsHostServices();
builder.Services.AddFunctionsInfrastructureServices();


var host = builder.Build();
await host.RunAsync();
