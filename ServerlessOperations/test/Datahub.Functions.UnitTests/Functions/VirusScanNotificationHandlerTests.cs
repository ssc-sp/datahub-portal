using Azure.Core;
using Azure.Messaging.ServiceBus;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

namespace Datahub.Functions.UnitTests.Functions;

public class VirusScanNotificationHandlerTests
{
    private readonly ILogger<VirusScanNotificationHandler> _logger = Substitute.For<ILogger<VirusScanNotificationHandler>>();

    [Test]
    public async Task RunAsync_WhenScanIsClean_QueuesStatusAndMovesBlobToUsersContainer()
    {
        var sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        var userGuid = Guid.NewGuid().ToString();
        var dbContextFactory = CreateDbContextFactoryForVirusScanning(userGuid);
        var endpoint = Substitute.For<ISendEndpoint>();
        sendEndpointProvider.GetSendEndpoint(Arg.Any<Uri>()).Returns(Task.FromResult(endpoint));

        var storageManagementService = Substitute.For<IWorkspaceStorageManagementService>();
        storageManagementService.MoveBlobToUsersContainerAsync(Arg.Any<string>(), Arg.Any<TokenCredential>())
            .Returns("users/test-file.txt");

        var sut = CreateHandler(
            sendEndpointProvider: sendEndpointProvider,
            storageManagementService: storageManagementService,
            dbContextFactory: dbContextFactory);

        var scanMessage = new ClamAVMessage
        {
            ScanError = string.Empty,
            ScanEndTime = DateTime.UtcNow,
            ScannedFile = "https://storage.test.blob.core.windows.net/external-uploads/user@example.com/test-file.txt"
        };

        await sut.RunAsync(CreateServiceBusMessage(scanMessage));

        await storageManagementService.Received(1).MoveBlobToUsersContainerAsync(
            scanMessage.ScannedFile,
            Arg.Any<TokenCredential>());

        await sendEndpointProvider.Received(1).GetSendEndpoint(
            Arg.Is<Uri>(uri => uri.Scheme == "queue" && uri.AbsolutePath == QueueConstants.VirusScanStatusQueueName));
    }

    [Test]
    public async Task RunAsync_WhenScanIsInfected_SendsNotificationsAndLocksUser()
    {
        var userGuid = Guid.NewGuid().ToString();
        var dbContextFactory = CreateDbContextFactoryForVirusScanning(userGuid);
        var gcNotifyService = Substitute.For<IGCNotifyService>();
        var lockedUserManagementService = Substitute.For<ILockedUserManagementService>();
        var sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        sendEndpointProvider.GetSendEndpoint(Arg.Any<Uri>()).Returns(Task.FromResult(Substitute.For<ISendEndpoint>()));

        var sut = CreateHandler(
            dbContextFactory: dbContextFactory,
            sendEndpointProvider: sendEndpointProvider,
            lockedUserManagementService: lockedUserManagementService,
            gcNotifyService: gcNotifyService);

        var scanMessage = new ClamAVMessage
        {
            ScanError = "Virus detected",
            ScanEndTime = DateTime.UtcNow,
            ScannedFile = "https://storage.test.blob.core.windows.net/external-uploads/test-file.txt",
            OriginalBlobMetadata = new ClamAVBlobMetadata { CreatedBy = userGuid }
        };

        await sut.RunAsync(CreateServiceBusMessage(scanMessage));

        await gcNotifyService.Received(1).SendInfectedFileNotification(
            IGCNotifyService.DEFAULT_MAILBOX,
            "test-file.txt",
            "storage",
            Arg.Any<string>());

        await gcNotifyService.Received(1).SendInfectedFileNotification(
            "lead@example.com",
            "test-file.txt",
            "storage",
            Arg.Any<string>());

        await lockedUserManagementService.Received(1).LockUserAsync(
            1,
            Arg.Is<string>(details => details.Contains("test-file.txt") && details.Contains("storage")),
            null);
    }

    [Test]
    public async Task RunAsync_WhenUploaderIsMissing_DoesNotLockUserButSendsNotifications()
    {
        var dbContextFactory = CreateDbContextFactoryForVirusScanning(Guid.NewGuid().ToString());
        var gcNotifyService = Substitute.For<IGCNotifyService>();
        var lockedUserManagementService = Substitute.For<ILockedUserManagementService>();

        var sut = CreateHandler(
            dbContextFactory: dbContextFactory,
            lockedUserManagementService: lockedUserManagementService,
            gcNotifyService: gcNotifyService);

        var scanMessage = new ClamAVMessage
        {
            ScanError = "Virus detected",
            ScanEndTime = DateTime.UtcNow,
            ScannedFile = "https://storage.test.blob.core.windows.net/just-a-file.txt",
            OriginalBlobMetadata = new ClamAVBlobMetadata { CreatedBy = string.Empty }
        };

        await sut.RunAsync(CreateServiceBusMessage(scanMessage));

        await gcNotifyService.Received(1).SendInfectedFileNotification(
            IGCNotifyService.DEFAULT_MAILBOX,
            "just-a-file.txt",
            "storage",
            Arg.Any<string>());

        await lockedUserManagementService.DidNotReceiveWithAnyArgs().LockUserAsync(default, default!, default);
    }

    [Test]
    public async Task RunAsync_WhenOriginalBlobMetadataIsNull_FallsBackToPathExtraction()
    {
        var gcNotifyService = Substitute.For<IGCNotifyService>();
        var lockedUserManagementService = Substitute.For<ILockedUserManagementService>();
        var sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        sendEndpointProvider.GetSendEndpoint(Arg.Any<Uri>()).Returns(Task.FromResult(Substitute.For<ISendEndpoint>()));

        var dbContextFactory = CreateDbContextFactoryForPathExtraction();

        var sut = CreateHandler(
            dbContextFactory: dbContextFactory,
            sendEndpointProvider: sendEndpointProvider,
            lockedUserManagementService: lockedUserManagementService,
            gcNotifyService: gcNotifyService);

        var scanMessage = new ClamAVMessage
        {
            ScanError = "Virus detected",
            ScanEndTime = DateTime.UtcNow,
            ScannedFile = $"https://storage.test.blob.core.windows.net/external-uploads/user_example.com/test-file.txt",
            OriginalBlobMetadata = null
        };

        await sut.RunAsync(CreateServiceBusMessage(scanMessage));

        await gcNotifyService.Received(1).SendInfectedFileNotification(
            IGCNotifyService.DEFAULT_MAILBOX,
            "test-file.txt",
            "storage",
            Arg.Any<string>());

        await lockedUserManagementService.Received(1).LockUserAsync(
            1,
            Arg.Is<string>(details => details.Contains("test-file.txt")),
            null);
    }

    private static VirusScanNotificationHandler CreateHandler(
        IDbContextFactory<DatahubProjectDBContext>? dbContextFactory = null,
        ISendEndpointProvider? sendEndpointProvider = null,
        ILockedUserManagementService? lockedUserManagementService = null,
        ISystemTokenCredentialService? systemTokenCredentialService = null,
        IGCNotifyService? gcNotifyService = null,
        IWorkspaceStorageManagementService? storageManagementService = null)
    {
        return new VirusScanNotificationHandler(
            Substitute.For<ILogger<VirusScanNotificationHandler>>(),
            dbContextFactory ?? Substitute.For<IDbContextFactory<DatahubProjectDBContext>>(),
            sendEndpointProvider ?? Substitute.For<ISendEndpointProvider>(),
            lockedUserManagementService ?? Substitute.For<ILockedUserManagementService>(),
            gcNotifyService ?? Substitute.For<IGCNotifyService>(),
            systemTokenCredentialService ?? Substitute.For<ISystemTokenCredentialService>(),
            storageManagementService ?? Substitute.For<IWorkspaceStorageManagementService>());
    }


    private static ServiceBusReceivedMessage CreateServiceBusMessage(ClamAVMessage message)
    {
        var payload = JsonSerializer.Serialize(message);
        return ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(payload));
    }

    private static IDbContextFactory<DatahubProjectDBContext> CreateDbContextFactoryForVirusScanning(string userGuid)
    {
        var options = new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var contextFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
        contextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<DatahubProjectDBContext>(new SqlServerDatahubContext(options)));

        SeedContextAsync(userGuid, options).GetAwaiter().GetResult();

        return contextFactory;
    }

    private static IDbContextFactory<DatahubProjectDBContext> CreateDbContextFactoryForPathExtraction()
    {
        var options = new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var contextFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
        contextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<DatahubProjectDBContext>(new SqlServerDatahubContext(options)));

        SeedContextForPathExtractionAsync(options).GetAwaiter().GetResult();

        return contextFactory;
    }

    private static async Task SeedContextAsync(string userGuid, DbContextOptions<SqlServerDatahubContext> options)
    {
        await using var context = new SqlServerDatahubContext(options);

        context.Projects.Add(new Datahub_Project
        {
            Project_Acronym_CD = "storage",
            Project_Name = "Test Workspace",
            Project_Status_Desc = "Active",
            UserRoles =
            [
                new UserRoleLinks
                {
                    RoleId = (int)Project_Role.RoleNames.WorkspaceLead,
                    PortalUser = new PortalUser { Id = 2, Email = "lead@example.com" }
                }
            ]
        });

        context.EntraUsers.Add(new EntraUser
        {
            GraphGuid = userGuid,
            PortalUser = new PortalUser { Id = 1, Email = "uploader@example.com" }
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedContextForPathExtractionAsync(DbContextOptions<SqlServerDatahubContext> options)
    {
        await using var context = new SqlServerDatahubContext(options);

        context.Projects.Add(new Datahub_Project
        {
            Project_Acronym_CD = "unknown",
            Project_Name = "Test Workspace",
            Project_Status_Desc = "Active",
            UserRoles =
            [
                new UserRoleLinks
                {
                    RoleId = (int)Project_Role.RoleNames.WorkspaceLead,
                    PortalUser = new PortalUser { Id = 2, Email = "lead@example.com" }
                }
            ]
        });

        context.PortalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "user@example.com"
        });

        await context.SaveChangesAsync();
    }
}
