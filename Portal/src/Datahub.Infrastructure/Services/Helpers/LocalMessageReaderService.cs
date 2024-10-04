using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Health;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services.Helpers;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Datahub.Infrastructure.Services;

public class LocalMessageReaderService : BackgroundService
{
    private readonly ILogger<LocalMessageReaderService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private FileSystemWatcher _watcher;

    public LocalMessageReaderService(
            ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<LocalMessageReaderService>();
    }

    private static string MessageFolder => Path.Combine(Directory.GetCurrentDirectory(), "MessageFolder");

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var folderPath = MessageFolder;
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        _watcher = new FileSystemWatcher
        {
            Path = folderPath,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*"
        };

        _watcher.Created += OnCreated;
        _watcher.EnableRaisingEvents = true;

        return Task.CompletedTask;
    }

    private static bool IsEmptyMessage(InfrastructureHealthCheckMessage? message) => message is null 
        || (message.Group == default && message.Name == default && message.Type == default);

    
    private static bool IsInvalidResultMessage(InfrastructureHealthCheckResultMessage? message) => message is null || message.HealthCheckTimeUtc == default;

    private async void OnCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"New file created: {e.Name}");

        // Wait until the file is no longer locked by the other process
        while (IsFileLocked(e.FullPath))
        {
            Thread.Sleep(100);
        }

        // Create a new scope to retrieve scoped services
        using (var scope = _serviceProvider.CreateScope())
        { 
            var projectStorageConfigurationService = scope.ServiceProvider.GetRequiredService<ProjectStorageConfigurationService>();
            var projectDBContext = scope.ServiceProvider.GetRequiredService<DatahubProjectDBContext>(); 
            var portalConfiguration = scope.ServiceProvider.GetRequiredService<DatahubPortalConfiguration>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>(); 
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>(); 
            var resourceMessagingService = scope.ServiceProvider.GetRequiredService<IResourceMessagingService>();
            var devopsConfig = configuration
                .GetSection("InfrastructureRepository:AzureDevOpsConfiguration")
                .Get<AzureDevOpsConfiguration>();
            string fileContents = File.ReadAllText(e.FullPath);

            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DatahubProjectDBContext>>();
            var sendEndpointProvider = scope.ServiceProvider.GetRequiredService<ISendEndpointProvider>();

            var healthCheckHelper = new HealthCheckHelper(dbContextFactory, projectStorageConfigurationService, configuration, 
                httpClientFactory, _loggerFactory, sendEndpointProvider, resourceMessagingService, portalConfiguration);

            // Deserialize the file contents into an InfrastructureHealthCheckMessage object
            InfrastructureHealthCheckMessage message = JsonSerializer.Deserialize<InfrastructureHealthCheckMessage>(fileContents);
            InfrastructureHealthCheckResultMessage checkResult = JsonSerializer.Deserialize<InfrastructureHealthCheckResultMessage>(fileContents);

            if (!IsEmptyMessage(message))
            {
                var results = await healthCheckHelper.ProcessHealthCheckRequest(message!);
                _logger.LogInformation($"Completed {results.Count()} health checks.");

                if (results != null)
                {
                    var bugReports = results.Select(r => healthCheckHelper.CreateBugReportMessage(r.Check));
                    _logger.LogInformation($"Generated {bugReports.Count()} bug reports.");

                    await Task.WhenAll(bugReports
                        .Where(r => r is not null)
                        .Cast<BugReportMessage>()
                        .Select(SaveBugReport));
                }
            }

            if (!IsInvalidResultMessage(checkResult))
            {
               await RecordHealthCheckResultsUsingHelper(healthCheckHelper, checkResult!);
            }
        }
    }

    private static bool IsFileLocked(string filePath)
    {
        try
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return false;
            }
        }
        catch (IOException)
        {
            // If an IOException is thrown, it means the file is still locked
            return true;
        }
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _watcher.Dispose();
        return base.StopAsync(stoppingToken);
    }

    //  copied and modified from \datahub-portal\ServerlessOperations\src\Datahub.Functions\CheckInfrastructureStatus.cs
    //  to facilitate local debugging

    private static string GenerateBugReportFilename(BugReportMessage bugReportMessage)
    {
        var identifier = string.IsNullOrEmpty(bugReportMessage.Topics) ? "unknown" : bugReportMessage.Topics;
        var timestamp = DateTime.Now.ToString("s");
        var filename = $"{timestamp}_{identifier}_bug_report";
        var cleanFilename = Regex.Replace(filename, "\\W+", "_");
        return $"{cleanFilename}.txt";
    }

    private static async Task SaveBugReport(BugReportMessage bugReportMessage)
    {
        var filename = GenerateBugReportFilename(bugReportMessage);
        var outputPath = Path.Combine(MessageFolder, filename);
        var message = JsonSerializer.Serialize(bugReportMessage);
        await File.WriteAllTextAsync(outputPath, message);
    }

    #region Handling Health Check Results 
    
    private async Task RecordHealthCheckResultsUsingHelper(HealthCheckHelper helper, InfrastructureHealthCheckResultMessage message)
    {
        var result = new InfrastructureHealthCheck() 
        {
            Details = message.Details,
            Group = message.Group,
            HealthCheckTimeUtc = message.HealthCheckTimeUtc,
            Name = message.Name,
            ResourceType = message.ResourceType,
            Status = message.Status,
            // no url
        };

        try
        {
            await helper.StoreHealthCheck(result);
            await helper.StoreHealthCheckRun(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving result (name: {result.Name}; group: {result.Group}; type: {result.ResourceType})");
        }
    }

    #endregion
}
