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
using Datahub.Application.Services;
using Datahub.Application.Services.Projects;
using Datahub.Application.Services.Security;
using Datahub.Functions.Services;
using Datahub.Functions.Providers;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Services.Security;

var host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureAppConfiguration(builder =>
	{
		builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.Build();
	})
	.ConfigureServices((hostContext, services) =>
	{
		var config = hostContext.Configuration;

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
		services.AddSingleton<IKeyVaultService, KeyVaultCoreService>();
		services.AddSingleton<IEmailService, EmailService>();
		services.AddScoped<ProjectUsageService>();
		services.AddScoped<QueuePongService>();
		services.AddScoped<IResourceMessagingService, ResourceMessagingService>();
		services.AddScoped<IProjectInactivityNotificationService, ProjectInactivityNotificationService>();
		services.AddScoped<IUserInactivityNotificationService, UserInactivityNotificationService>();
		services.AddScoped<IDateProvider, DateProvider>();
		services.AddScoped<EmailValidator>();

		services.AddDatahubConfigurationFromFunctionFormat(config);


	})
	.Build();

host.Run();
