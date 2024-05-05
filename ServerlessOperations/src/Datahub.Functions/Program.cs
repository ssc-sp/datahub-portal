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
using MassTransit;
using Datahub.Application.Services;
using Datahub.Application.Services.Projects;
using Datahub.Application.Services.Security;
using Datahub.Functions.Services;
using Datahub.Functions.Providers;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Services.Security;
using Microsoft.Extensions.Azure;

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

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Datahub.Infrastructure.ConfigureServices).Assembly));

        services.AddHttpClient(AzureManagementService.ClientName).AddPolicyHandler(
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(x => x.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(2), 5)));

        services.AddSingleton<AzureConfig>();
        services.AddSingleton<IAzureServicePrincipalConfig, AzureConfig>();
        services.AddSingleton<AzureManagementService>();
        services.AddAzureClients(
            builder =>
            {
                builder.AddClient<ArmClient, ArmClientOptions>(options =>
                {
                    options.Diagnostics.IsLoggingEnabled = true;
                    options.Retry.Mode = RetryMode.Exponential;
                    options.Retry.MaxRetries = 5;
                    options.Retry.Delay = TimeSpan.FromSeconds(2);
                    var tenantId = config.GetValue<string>("TENANT_ID");
                    var clientId = config.GetValue<string>("FUNC_SP_CLIENT_ID");
                    var clientSecret = config.GetValue<string>("FUNC_SP_CLIENT_SECRET");
                    var subscriptionId = config.GetValue<string>("SUBSCRIPTION_ID");
                    var creds = new ClientSecretCredential(tenantId, clientId, clientSecret);
                    var client = new ArmClient(creds, subscriptionId, options);
                    return client;
                });
            }
            );
        services.AddSingleton<IKeyVaultService, KeyVaultCoreService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddScoped<ProjectUsageService>();
        services.AddScoped<QueuePongService>();
        services.AddScoped<IResourceMessagingService, ResourceMessagingService>();
        services.AddScoped<IProjectInactivityNotificationService, ProjectInactivityNotificationService>();
        services.AddScoped<IUserInactivityNotificationService, UserInactivityNotificationService>();
        services.AddScoped<IDateProvider, DateProvider>();
        services.AddScoped<EmailValidator>();
        services.AddScoped<EmailNotificationHandler>(); // add your functions as scoped
        services.AddMassTransitForAzureFunctions(cfg =>
                {
                    cfg.AddConsumersFromNamespaceContaining<EmailNotificationConsumer>();
                }, "MassTransit:AzureServiceBus:ConnectionString");
        services.AddDatahubConfigurationFromFunctionFormat(config);


    })
    .Build();

host.Run();
