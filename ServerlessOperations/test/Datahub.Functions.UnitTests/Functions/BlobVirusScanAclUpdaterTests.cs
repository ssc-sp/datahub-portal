using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Datahub.Application.Services.Storage;
using Datahub.Functions;
using Datahub.Functions.Models;
using Datahub.Functions.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;

namespace Datahub.Functions.UnitTests.Functions;

[TestFixture]
public class BlobVirusScanAclUpdaterTests
{
    private IWorkspaceAclService _workspaceAclService = null!;
    private IBlobMetadataWriter _blobMetadataWriter = null!;
    private BlobVirusScanAclUpdater _function = null!;

    [SetUp]
    public void SetUp()
    {
        _workspaceAclService = Substitute.For<IWorkspaceAclService>();
        _blobMetadataWriter = Substitute.For<IBlobMetadataWriter>();
        _function = new BlobVirusScanAclUpdater(
            NullLogger<BlobVirusScanAclUpdater>.Instance,
            _workspaceAclService,
            _blobMetadataWriter);
    }

    [Test]
    public async Task RunAsync_AppliesAcls_WhenScanStatusIsClean()
    {
        var data = new BlobMetadataEventData
        {
            Url = "https://storageaccount.dfs.core.windows.net/datahub/upload/abc/myfile.txt",
            Metadata = new Dictionary<string, string>
            {
                ["dh:scanStatus"] = "Clean"
            }
        };

        var eventGridEvent = new EventGridEvent(
            subject: "/blobServices/default/containers/datahub/blobs/upload/abc/myfile.txt",
            eventType: "Microsoft.Storage.BlobPropertiesUpdated",
            dataVersion: "1.0",
            data: BinaryData.FromObjectAsJson(data));

        await _function.RunAsync(eventGridEvent, context: null!, CancellationToken.None);

        await _workspaceAclService
            .Received(1)
            .ApplyWorkspaceMemberAclsAsync(
                "ABC",
                "upload/abc/myfile.txt",
                "r--",
                false);

        await _blobMetadataWriter
            .Received(1)
            .SetAccessEnabledMetadataAsync(
                "ABC",
                "upload/abc/myfile.txt",
                Arg.Is<IReadOnlyDictionary<string, string>>(m => m["dh:scanStatus"] == "Clean"),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RunAsync_Skips_WhenScanStatusNotClean()
    {
        var data = new BlobMetadataEventData
        {
            Url = "https://storageaccount.dfs.core.windows.net/datahub/upload/abc/myfile.txt",
            Metadata = new Dictionary<string, string>
            {
                ["dh:scanStatus"] = "Infected"
            }
        };

        var eventGridEvent = new EventGridEvent(
            "/blobServices/default/containers/datahub/blobs/upload/abc/myfile.txt",
            "Microsoft.Storage.BlobPropertiesUpdated",
            "1.0",
            BinaryData.FromObjectAsJson(data));

        await _function.RunAsync(eventGridEvent, context: null!, CancellationToken.None);

        await _workspaceAclService.DidNotReceiveWithAnyArgs().ApplyWorkspaceMemberAclsAsync(default!, default!, default!, default);
        await _blobMetadataWriter.DidNotReceiveWithAnyArgs().SetAccessEnabledMetadataAsync(default!, default!, default!, default);
    }

    [Test]
    public async Task RunAsync_Skips_WhenPathOutsideUpload()
    {
        var data = new BlobMetadataEventData
        {
            Url = "https://storageaccount.dfs.core.windows.net/datahub/raw/abc/myfile.txt",
            Metadata = new Dictionary<string, string>
            {
                ["dh:scanStatus"] = "Clean"
            }
        };

        var eventGridEvent = new EventGridEvent(
            "/blobServices/default/containers/datahub/blobs/raw/abc/myfile.txt",
            "Microsoft.Storage.BlobPropertiesUpdated",
            "1.0",
            BinaryData.FromObjectAsJson(data));

        await _function.RunAsync(eventGridEvent, context: null!, CancellationToken.None);

        await _workspaceAclService.DidNotReceiveWithAnyArgs().ApplyWorkspaceMemberAclsAsync(default!, default!, default!, default);
        await _blobMetadataWriter.DidNotReceiveWithAnyArgs().SetAccessEnabledMetadataAsync(default!, default!, default!, default);
    }
}
