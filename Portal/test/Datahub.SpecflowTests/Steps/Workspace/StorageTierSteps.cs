using Bunit;
using Datahub.Application.Services.Security;
using Datahub.Core.Components.Code;
using Datahub.Core.Data;
using Datahub.Core.Model.Projects;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Api;
using Datahub.Portal.Pages.Workspace.Storage;
using Datahub.SpecflowTests.Utils;
using Datahub.Shared.Entities;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Application.Services.Publishing;
using Datahub.Application.Services.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;
using System.Reflection;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class StorageTierSteps : BunitTestSteps
{
    private const string ContainerName = "container";
    private readonly ICloudStorageManager _storageManager = Substitute.For<ICloudStorageManager>();
    private readonly Dictionary<string, string?> _tiers = new(StringComparer.Ordinal);
    private readonly List<string> _requestedPaths = [];
    private IRenderedComponent<FileItem>? _fileItem;
    private IRenderedComponent<FileProperties>? _fileProperties;
    private List<string> _paths = [];
    private List<PortalFileMetadata> _metadataFiles = [];
    private bool _tierCheckResult;
    private string? _itemTier;
    private bool _folder;
    private StorageHeading? _heading;
    private readonly ISnackbar _snackbar = Substitute.For<ISnackbar>();
    private readonly IJSObjectReference _headingModule = Substitute.For<IJSObjectReference>();
    private readonly IStringLocalizer _localizer = Substitute.For<IStringLocalizer>();
    private readonly List<string> _changedTiers = [];
    private readonly List<string> _downloadedFiles = [];
    private bool _archiveConfirmation = true;
    private CloudStorageArchiveState _archiveState = CloudStorageArchiveState.Available;
    private TaskCompletionSource<bool>? _pendingUpdate;
    private Task? _runningUpdate;
    private IRenderedComponent<FileExplorer>? _explorer;

    public StorageTierSteps()
    {
        JSInterop.SetupMudBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(Substitute.For<IDialogService>());

        _localizer[Arg.Any<string>()].Returns(call =>
            new LocalizedString(call.Arg<string>(), call.Arg<string>()));
        _localizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(call =>
            new LocalizedString(
                call.ArgAt<string>(0),
                string.Format(call.ArgAt<string>(0), call.ArgAt<object[]>(1))));
        Services.AddSingleton(_localizer);

        var featureManager = Substitute.For<IFeatureManagerSnapshot>();
        featureManager.IsEnabledAsync(Arg.Any<string>()).Returns(false);
        Services.AddSingleton(featureManager);

        var apiTargets = new APITargets { FileSystemName = "filesystem" };
        Services.AddSingleton(new CommonAzureServices(
            Substitute.For<ILogger<CommonAzureServices>>(),
            Substitute.For<IKeyVaultCoreService>(),
            Options.Create(apiTargets)));

        ComponentFactories.AddStub<InlineCodeWithCopy>();
        ComponentFactories.AddStub<FileMetadataEditor>();

        _storageManager.GetFileStorageTierAsync(ContainerName, Arg.Any<string>())
            .Returns(call =>
            {
                var path = call.ArgAt<string>(1);
                _requestedPaths.Add(path);
                return _tiers.TryGetValue(path, out var tier) ? tier : "Hot";
            });
        _storageManager.SetFileStorageTierAsync(ContainerName, Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _storageManager.GetFileStorageTiersList().Returns(["Hot", "Cool", "Cold", "Archive"]);
        _storageManager
            .GetFileArchiveStatusAsync(ContainerName, Arg.Any<string>())
            .Returns(_ => new CloudStorageArchiveStatus(_archiveState));
        _headingModule.InvokeAsync<bool>(Arg.Any<string>(), Arg.Any<object?[]?>())
            .Returns(_ => new ValueTask<bool>(_archiveConfirmation));
    }

    [Given("a file item with tier {string}")]
    public void GivenAFileItemWithTier(string tier)
    {
        _folder = false;
        _itemTier = tier;
    }

    [Given("a file item with no storage tier")]
    public void GivenAFileItemWithNoStorageTier()
    {
        _folder = false;
        _itemTier = string.Empty;
    }

    [Given("a folder item with tier {string}")]
    public void GivenAFolderItemWithTier(string tier)
    {
        _folder = true;
        _itemTier = tier;
    }

    [When("the file item is rendered")]
    public void WhenTheFileItemIsRendered()
    {
        _fileItem = Render<FileItem>(parameters => parameters
            .Add(component => component.Name, "report.csv")
            .Add(component => component.Size, "1024")
            .Add(component => component.Modified, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc))
            .Add(component => component.Folder, _folder)
            .Add(component => component.Highlighted, true)
            .Add(component => component.StorageTier, _itemTier!));
    }

    [Then("the file item should display {string}")]
    public void ThenTheFileItemShouldDisplay(string text) => _fileItem!.Markup.Should().Contain(text);

    [Then("the file item should display its name size and modified date")]
    public void ThenTheFileItemShouldDisplayItsOtherProperties()
    {
        _fileItem!.Markup.Should().Contain("report.csv");
        _fileItem.Markup.Should().Contain("1.02 KB");
        _fileItem.Markup.Should().Contain("highlight");
        _fileItem.Markup.Should().Contain("Fri, 02 Jan 2026 03:04:05 GMT");
    }

    [Then("the file item should not display a storage tier suffix")]
    public void ThenTheFileItemShouldNotDisplayAStorageTierSuffix()
    {
        _fileItem!.Find(".file-item-size").TextContent.Should().NotContain("(");
    }

    [Given("file properties with tier {string}")]
    public void GivenFilePropertiesWithTier(string tier)
    {
        _itemTier = tier;
        _storageManager.ProviderType.Returns(tier is "NEARLINE" or "COLDLINE" or "ARCHIVE"
            ? CloudStorageProviderType.GCP
            : AWSCloudStorageManager.GetStorageClassLabel(tier) != tier
                ? CloudStorageProviderType.AWS
                : CloudStorageProviderType.Azure);
    }

    [When("the file properties are rendered")]
    public void WhenTheFilePropertiesAreRendered()
    {
        var project = new Datahub_Project
        {
            Project_Name = "Test",
            Project_Acronym_CD = "TEST",
            Data_Sensitivity = Datahub.Metadata.Model.ClassificationType.Unclassified
        };
        var storageMetadata = new StorageMetadata { Container = ContainerName };
        var file = new FileMetadata { id = "file-id", filename = "report.csv", folderpath = "folder", filesize = "10" };

        _fileProperties = Render<FileProperties>(parameters => parameters
            .AddCascadingValue("ProjectAcronym", "TEST")
            .AddCascadingValue("Project", project)
            .AddCascadingValue("StorageAccountMetadata", storageMetadata)
            .Add(component => component.File, file)
            .Add(component => component.StorageTier, _itemTier!)
            .Add(component => component.StorageManager, _storageManager)
            .Add(component => component.ContainerName, ContainerName));
    }

    [When("the file properties tier changes to {string}")]
    public void WhenTheFilePropertiesTierChangesTo(string tier)
    {
        _fileProperties!.Render(parameters => parameters.Add(component => component.StorageTier, tier));
    }

    [Then("the file properties should display storage tier {string}")]
    public void ThenTheFilePropertiesShouldDisplayStorageTier(string tier)
    {
        _fileProperties!.FindAll(".properties .text").Select(element => element.TextContent)
            .Should().Contain(tier);
    }

    [Then("the archive warning should be {word}")]
    public void ThenTheArchiveWarningShouldBe(string visibility)
    {
        var visible = _fileProperties!.Markup.Contains("Archived Storage", StringComparison.Ordinal);
        visible.Should().Be(visibility == "visible");
    }

    [Given("the following file tiers")]
    public void GivenTheFollowingFileTiers(Table table) => ConfigureTiers(table, metadataFiles: false);

    [Given("metadata files in tiers")]
    public void GivenMetadataFilesInTiers(Table table) => ConfigureTiers(table, metadataFiles: true);

    [Given("no files to check")]
    public void GivenNoFilesToCheck()
    {
        _paths = [];
        _metadataFiles = [];
    }

    [When("direct paths are checked for tier {string}")]
    public async Task WhenDirectPathsAreCheckedForTier(string tier)
    {
        _tierCheckResult = await StorageHeading.CheckIfAnyFilesInTiers(
            _paths, [tier], _storageManager, ContainerName);
    }

    [When("metadata files are checked for tier {string}")]
    public async Task WhenMetadataFilesAreCheckedForTier(string tier)
    {
        _tierCheckResult = await StorageHeading.CheckIfAnyFilesInTiers(
            _metadataFiles, [tier], _storageManager, ContainerName);
    }

    [Then("the tier check should succeed")]
    public void ThenTheTierCheckShouldSucceed() => _tierCheckResult.Should().BeTrue();

    [Then("the tier check should fail")]
    public void ThenTheTierCheckShouldFail() => _tierCheckResult.Should().BeFalse();

    [Then("tier lookup should stop after {string}")]
    public void ThenTierLookupShouldStopAfter(string path)
    {
        _requestedPaths.Should().EndWith(path);
        _requestedPaths.Should().NotContain("folder/cold.csv");
    }

    [Then("storage tiers should be requested for the metadata file paths")]
    public void ThenStorageTiersShouldBeRequestedForMetadataFilePaths()
    {
        _requestedPaths.Should().Equal(_metadataFiles.Select(file => file.fullPathFromRoot));
    }

    [Then("no storage tier should be requested")]
    public void ThenNoStorageTierShouldBeRequested() => _requestedPaths.Should().BeEmpty();

    private void ConfigureTiers(Table table, bool metadataFiles)
    {
        foreach (var row in table.Rows)
        {
            _tiers[row["Path"]] = row["Tier"];
        }

        _paths = table.Rows.Select(row => row["Path"]).ToList();
        if (metadataFiles)
        {
            _metadataFiles = _paths.Select(path => new PortalFileMetadata { id = path, filename = path }).ToList();
        }
    }

    [Given("a storage heading in folder {string} with selected file {string}")]
    public void GivenAStorageHeadingWithSelectedFile(string folder, string filename)
    {
        ConfigureHeading(folder,
            [new PortalFileMetadata { id = filename, filename = filename }],
            [filename]);
    }

    [Given("a storage heading with selected files and a folder")]
    public void GivenAStorageHeadingWithSelectedFilesAndAFolder()
    {
        ConfigureHeading("/",
            [
                new PortalFileMetadata { id = "a.csv", filename = "a.csv" },
                new PortalFileMetadata { id = "b.csv", filename = "b.csv" }
            ],
            ["a.csv", "folder/", "b.csv"]);
    }

    [Given("a storage heading with selected files and a failed tier update")]
    public void GivenAStorageHeadingWithSelectedFilesAndAFailedTierUpdate()
    {
        GivenAStorageHeadingWithSelectedFilesAndAFolder();
        _storageManager.SetFileStorageTierAsync(ContainerName, "b.csv", Arg.Any<string>()).Returns(false);
    }

    [Given("the archive warning is cancelled")]
    public void GivenTheArchiveWarningIsCancelled() => _archiveConfirmation = false;

    [Given("the archive warning is confirmed")]
    public void GivenTheArchiveWarningIsConfirmed() => _archiveConfirmation = true;

    [When("the heading changes the tier to {string}")]
    public async Task WhenTheHeadingChangesTheTierTo(string tier)
    {
        await InvokeHeadingMethod("HandleTierChange", tier);
    }

    [Then("tier {string} should be persisted for path {string}")]
    public async Task ThenTierShouldBePersistedForPath(string tier, string path)
    {
        await _storageManager.Received(1).SetFileStorageTierAsync(ContainerName, path, tier);
    }

    [Then("the storage tier change callback should receive {string}")]
    public void ThenTheStorageTierChangeCallbackShouldReceive(string tier) => _changedTiers.Should().Equal(tier);

    [Then("a successful tier change should be reported")]
    public void ThenASuccessfulTierChangeShouldBeReported()
    {
        _snackbar.Received().Add(Arg.Is<string>(message => message.Contains("successfully", StringComparison.Ordinal)),
            Severity.Success);
    }

    [Then("a successful tier change to {string} should be reported")]
    public void ThenASuccessfulTierChangeToShouldBeReported(string label)
    {
        _snackbar.Received().Add(
            Arg.Is<string>(message => message.Contains(label, StringComparison.Ordinal)
                && message.Contains("successfully", StringComparison.Ordinal)),
            Severity.Success);
    }

    [Then("only the selected files should be changed to tier {string}")]
    public async Task ThenOnlyTheSelectedFilesShouldBeChangedToTier(string tier)
    {
        await _storageManager.Received(1).SetFileStorageTierAsync(ContainerName, "a.csv", tier);
        await _storageManager.Received(1).SetFileStorageTierAsync(ContainerName, "b.csv", tier);
        await _storageManager.DidNotReceive().SetFileStorageTierAsync(ContainerName, "folder/", tier);
    }

    [Then("every selected file tier update should be attempted")]
    public async Task ThenEverySelectedFileTierUpdateShouldBeAttempted()
    {
        await _storageManager.Received(1).SetFileStorageTierAsync(ContainerName, "a.csv", "Cold");
        await _storageManager.Received(1).SetFileStorageTierAsync(ContainerName, "b.csv", "Cold");
    }

    [Then("a failed tier change should be reported")]
    public void ThenAFailedTierChangeShouldBeReported()
    {
        _snackbar.Received().Add(Arg.Is<string>(message => message.Contains("Failed", StringComparison.Ordinal)),
            Severity.Error);
    }

    [Then("no file tier should be changed")]
    public async Task ThenNoFileTierShouldBeChanged()
    {
        await _storageManager.DidNotReceiveWithAnyArgs().SetFileStorageTierAsync(default!, default!, default!);
    }

    [Then("no storage tier change callback should be emitted")]
    public void ThenNoStorageTierChangeCallbackShouldBeEmitted() => _changedTiers.Should().BeEmpty();

    [Then("the archive warning should be requested once")]
    public void ThenTheArchiveWarningShouldBeRequestedOnce()
    {
        _headingModule.Received(1).InvokeAsync<bool>("confirmStorageTierChange", Arg.Any<object?[]?>());
    }

    private void ConfigureHeading(
        string folder,
        List<PortalFileMetadata> files,
        HashSet<string> selectedItems)
    {
        _heading = new StorageHeading
        {
            CurrentFolder = folder,
            ContainerName = ContainerName,
            Files = files,
            Folders = ["folder/"],
            SelectedItems = selectedItems,
            StorageManager = _storageManager,
            SelectedStorageTier = "Hot",
            OnStorageTierChanged = EventCallback.Factory.Create<string>(this, tier => _changedTiers.Add(tier))
        };

        SetPrivateMember(_heading, "_module", _headingModule);
        SetPrivateMember(_heading, "_snackbar", _snackbar);
        SetPrivateMember(_heading, "Localizer", _localizer);
        SetPrivateMember(_heading, "_currentUserRole", Project_Role.GetAll().First(role => role.IsAtLeastCollaborator));
        SetPrivateMember(_heading, "_selectedFiles", files);
        _heading.OnFileDownload = EventCallback.Factory.Create<string>(this, path => _downloadedFiles.Add(path));
        SetPrivateMember(_heading, "_telemetryService", Substitute.For<IPortalUserTelemetryService>());
    }

    [Given("the heading uses AWS storage")]
    public void GivenTheHeadingUsesAwsStorage()
    {
        _storageManager.ProviderType.Returns(CloudStorageProviderType.AWS);
        _storageManager.GetFileStorageTiersList().Returns(
            new AWSCloudStorageManager("account", "key", "secret", "ca-central-1", "bucket").GetFileStorageTiersList());
    }

    [Given("the heading uses GCP storage")]
    public void GivenTheHeadingUsesGcpStorage()
    {
        _storageManager.ProviderType.Returns(CloudStorageProviderType.GCP);
        _storageManager.GetFileStorageTiersList().Returns(["STANDARD", "NEARLINE", "COLDLINE", "ARCHIVE"]);
    }

    [When("storage class options are refreshed")]
    public void WhenStorageClassOptionsAreRefreshed() => InvokeHeadingVoid("RefreshStorageTierOptions");

    [Then("the heading offers AWS classes without Azure or legacy destinations")]
    public void ThenTheHeadingOffersAwsClasses()
    {
        GetHeadingField<List<string>>("_storageTiers").Should().Equal(
            "STANDARD", "STANDARD_IA", "ONEZONE_IA", "INTELLIGENT_TIERING", "GLACIER_IR", "GLACIER", "DEEP_ARCHIVE");
        _heading!.SelectedStorageTier.Should().BeEmpty();
    }

    [When("the heading switches to an Azure container")]
    public void WhenTheHeadingSwitchesToAzure()
    {
        var azure = Substitute.For<ICloudStorageManager>();
        azure.GetFileStorageTiersList().Returns(["Hot", "Cool", "Cold", "Archive"]);
        _heading!.StorageManager = azure;
        _heading.ContainerName = "azure-container";
        InvokeHeadingVoid("RefreshStorageTierOptions");
    }

    [Then("the heading offers Azure tiers with no selected AWS class")]
    public void ThenTheHeadingOffersAzureTiers()
    {
        GetHeadingField<List<string>>("_storageTiers").Should().Equal("Hot", "Cool", "Cold", "Archive");
        _heading!.SelectedStorageTier.Should().BeEmpty();
    }

    [When("the heading switches to a GCP container")]
    public void WhenTheHeadingSwitchesToGcp()
    {
        var gcp = Substitute.For<ICloudStorageManager>();
        gcp.ProviderType.Returns(CloudStorageProviderType.GCP);
        gcp.GetFileStorageTiersList().Returns(["STANDARD", "NEARLINE", "COLDLINE", "ARCHIVE"]);
        _heading!.StorageManager = gcp;
        _heading.ContainerName = "gcp-container";
        InvokeHeadingVoid("RefreshStorageTierOptions");
    }

    [Then("the heading offers GCP classes with no selected AWS class")]
    public void ThenTheHeadingOffersGcpClasses()
    {
        GetHeadingField<List<string>>("_storageTiers").Should().Equal("STANDARD", "NEARLINE", "COLDLINE", "ARCHIVE");
        _heading!.SelectedStorageTier.Should().BeEmpty();
    }

    [Then("no archive confirmation should be requested")]
    public void ThenNoArchiveConfirmationShouldBeRequested() => _headingModule
        .DidNotReceive().InvokeAsync<bool>("confirmStorageTierChange", Arg.Any<object?[]?>());

    [When("the heading downloads the selected files")]
    public Task WhenTheHeadingDownloadsTheSelectedFiles() => InvokeHeadingMethod("HandleDownload");

    [Then("the retrieval cost confirmation should be requested once")]
    public void ThenTheRetrievalCostConfirmationShouldBeRequestedOnce() => _headingModule.Received(1)
        .InvokeAsync<bool>("confirmDownloadCoolOrCold", Arg.Is<object?[]?>(args => args != null && args[0]!.ToString()!.Contains("increased cost")));

    [Then("no selected file should be downloaded")]
    public void ThenNoSelectedFileShouldBeDownloaded() => _downloadedFiles.Should().BeEmpty();

    [Given("the heading is read only")]
    public void GivenTheHeadingIsReadOnly() => _heading!.Readonly = true;

    [Given("the heading user is a guest")]
    public void GivenTheHeadingUserIsAGuest() => SetPrivateMember(_heading!, "_currentUserRole",
        Project_Role.GetAll().First(role => role.Id == (int)Project_Role.RoleNames.Guest));

    [Given("AWS archive availability is {word}")]
    public void GivenAwsArchiveAvailability(string state) => _archiveState = Enum.Parse<CloudStorageArchiveState>(state);

    [Given("reading AWS archive status fails")]
    public void GivenReadingAwsArchiveStatusFails() => _storageManager
        .GetFileArchiveStatusAsync(ContainerName, Arg.Any<string>())
        .Returns(Task.FromException<CloudStorageArchiveStatus>(new InvalidOperationException("Service unavailable")));

    [Then("AWS archive status should use path {string}")]
    public async Task ThenAwsArchiveStatusShouldUsePath(string path) => await _storageManager
        .Received().GetFileArchiveStatusAsync(ContainerName, path);

    [When("download availability is checked")]
    public async Task WhenDownloadAvailabilityIsChecked()
    {
        var method = typeof(StorageHeading).GetMethod("AnySelectedFileUnavailableAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        _tierCheckResult = await (Task<bool>)method.Invoke(_heading, null)!;
    }

    [Then("the selected file should be {word} for download")]
    public void ThenTheSelectedFileShouldBeAvailable(string availability) => _tierCheckResult.Should().Be(availability == "unavailable");

    [Then("AWS archive status should show {string}")]
    public void ThenAwsArchiveStatusShouldShow(string text) => _fileProperties!.Markup.Should().Contain(text);

    [When("AWS archive status is refreshed")]
    public async Task WhenAwsArchiveStatusIsRefreshed()
    {
        var method = typeof(FileProperties).GetMethod("RefreshArchiveStatusAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await _fileProperties!.InvokeAsync(() => (Task)method.Invoke(_fileProperties.Instance, null)!);
        _fileProperties.Render();
    }

    [Given("the selected file update is delayed")]
    public void GivenTheSelectedFileUpdateIsDelayed()
    {
        _pendingUpdate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _storageManager.SetFileStorageTierAsync(ContainerName, Arg.Any<string>(), Arg.Any<string>()).Returns(_pendingUpdate.Task);
    }

    [When("the AWS class change begins")]
    public void WhenTheAwsClassChangeBegins() => _runningUpdate = InvokeHeadingMethod("HandleTierChange", "STANDARD_IA");

    [When("the delayed update completes")]
    public async Task WhenTheDelayedUpdateCompletes()
    {
        _pendingUpdate!.SetResult(true);
        await _runningUpdate!;
    }

    [Given("the first selected file update throws")]
    public void GivenTheFirstSelectedFileUpdateThrows() => _storageManager
        .SetFileStorageTierAsync(ContainerName, "a.csv", Arg.Any<string>())
        .Returns(Task.FromException<bool>(new StorageTierChangeException("Files larger than 5 GB must have their storage class changed using AWS tooling.")));

    [Then("the heading should explain the 5 GB limit")]
    public void ThenTheHeadingShouldExplainTheLimit() => GetHeadingField<string?>("_tierError").Should().Contain("5 GB");

    private T GetHeadingField<T>(string name) => (T)typeof(StorageHeading)
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(_heading)!;

    private void InvokeHeadingVoid(string name) => typeof(StorageHeading)
        .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(_heading, null);

    [Given("an AWS file explorer with two selected Standard files and a partial update failure")]
    public void GivenAnAwsExplorerWithAPartialUpdateFailure()
    {
        GivenAStorageHeadingWithSelectedFilesAndAFolder();
        GivenTheHeadingUsesAwsStorage();
        _tiers["a.csv"] = "STANDARD";
        _tiers["b.csv"] = "STANDARD";
        _storageManager.SetFileStorageTierAsync(ContainerName, Arg.Any<string>(), Arg.Any<string>()).Returns(call =>
        {
            var path = call.ArgAt<string>(1);
            if (path != "a.csv") return false;
            _tiers[path] = call.ArgAt<string>(2);
            return true;
        });
        _storageManager.GetDfsPagesAsync(ContainerName, Arg.Any<string>(), Arg.Any<string?>())
            .Returns(new DfsPage([], _heading!.Files, null));
        _storageManager.ListFoldersAsync(ContainerName, Arg.Any<string>()).Returns(new Dictionary<string, int>());
        _storageManager.GetStorageMetadataAsync(ContainerName).Returns(new StorageMetadata { Container = ContainerName });
        var userInfo = Substitute.For<IUserInformationService>();
        userInfo.IsEntraUser().Returns(true);
        userInfo.GetCurrentUserEntraId().Returns("user-id");
        Services.AddSingleton(userInfo);
        Services.AddSingleton(Substitute.For<IDbContextFactory<DatahubProjectDBContext>>());
        Services.AddSingleton(Substitute.For<IPortalUserTelemetryService>());
        Services.AddSingleton(Substitute.For<IOpenDataPublishingService>());
        Services.AddSingleton(Substitute.For<IFileTokenService>());
        Services.AddSingleton(new DatahubPortalConfiguration());
        Services.AddSingleton(TimeProvider.System);
        Services.AddLogging();
        ComponentFactories.AddStub<StorageHeading>();
        ComponentFactories.AddStub<StorageProperties>();
        ComponentFactories.AddStub<FileProperties>();
        _explorer = Render<FileExplorer>(parameters => parameters
            .Add(component => component.Container, new CloudStorageContainer(_storageManager, ContainerName))
            .AddCascadingValue("ProjectAcronym", "TEST")
            .AddCascadingValue("PortalUser", new PortalUser { Email = "test@example.com" })
            .AddCascadingValue("Project", new Datahub_Project
            {
                Project_Acronym_CD = "TEST", Data_Sensitivity = Datahub.Metadata.Model.ClassificationType.Unclassified
            }));
        typeof(FileExplorer).GetField("_selectedItems", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(_explorer.Instance, new HashSet<string> { "a.csv", "b.csv" });
        _heading.OnStorageTierChanged = EventCallback.Factory.Create<string>(this, async tier =>
        {
            var method = typeof(FileExplorer).GetMethod("HandleStorageTierChanged", BindingFlags.Instance | BindingFlags.NonPublic)!;
            await _explorer.InvokeAsync(() => (Task)method.Invoke(_explorer.Instance, [tier])!);
        });
    }

    [Then("the explorer should display the persisted AWS classes after the partial failure")]
    public void ThenTheExplorerShouldDisplayPersistedAwsClasses()
    {
        _explorer!.WaitForAssertion(() =>
        {
            var items = _explorer.FindComponents<FileItem>().ToDictionary(item => item.Instance.Name, item => item.Instance.StorageTier);
            items["a.csv"].Should().Be("STANDARD_IA");
            items["b.csv"].Should().Be("STANDARD");
            typeof(FileExplorer).GetField("_selectedStorageTier", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(_explorer.Instance).Should().Be("");
        });
    }

    [When("the explorer loads another AWS container")]
    public void WhenTheExplorerLoadsAnotherAwsContainer()
    {
        var manager = Substitute.For<ICloudStorageManager>();
        manager.GetDfsPagesAsync("another-container", Arg.Any<string>(), Arg.Any<string?>())
            .Returns(new DfsPage([], _heading!.Files, null));
        manager.GetFileStorageTierAsync("another-container", Arg.Any<string>()).Returns("GLACIER_IR");
        _explorer!.Render(parameters => parameters.Add(component => component.Container,
            new CloudStorageContainer(manager, "another-container")));
    }

    [When("the explorer renames {string} to {string}")]
    public Task WhenTheExplorerRenames(string source, string destination) => RenameExplorerFileAsync(source, destination, true);

    [When("the explorer cannot rename {string} to {string}")]
    public Task WhenTheExplorerCannotRename(string source, string destination) => RenameExplorerFileAsync(source, destination, false);

    private async Task RenameExplorerFileAsync(string source, string destination, bool succeeds)
    {
        typeof(FileExplorer).GetField("_selectedItems", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(_explorer!.Instance, new HashSet<string> { source });
        _storageManager.RenameFileAsync(ContainerName, source, destination).Returns(succeeds);
        if (succeeds) _tiers[destination] = _tiers[source];
        var method = typeof(FileExplorer).GetMethod("HandleFileRename", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await _explorer.InvokeAsync(() => (Task)method.Invoke(_explorer.Instance, [destination])!);
        await _storageManager.Received(1).RenameFileAsync(ContainerName, source, destination);
    }

    [Then("the explorer should select {string}")]
    public void ThenTheExplorerShouldSelect(string filename) => ((HashSet<string>)typeof(FileExplorer)
        .GetField("_selectedItems", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(_explorer!.Instance)!)
        .Should().Equal(filename);

    [When("the explorer opens a nested folder with different AWS classes")]
    public async Task WhenTheExplorerOpensANestedFolder()
    {
        _tiers["nested/a.csv"] = "STANDARD_IA";
        _tiers["nested/b.csv"] = "ONEZONE_IA";
        var method = typeof(FileExplorer).GetMethod("SetCurrentFolder", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await _explorer!.InvokeAsync(() => (Task)method.Invoke(_explorer.Instance, ["nested/"])!);
    }

    [Then("the explorer should immediately display class {string} for {string}")]
    public void ThenTheExplorerShouldImmediatelyDisplayClass(string label, string filename)
    {
        _explorer!.WaitForAssertion(() => _explorer.FindComponents<FileItem>()
            .Single(item => item.Instance.Name == filename).Markup.Should().Contain($"({label})"));
    }

    [Then("no storage class update should have been required")]
    public async Task ThenNoStorageClassUpdateShouldHaveBeenRequired() => await _storageManager
        .DidNotReceiveWithAnyArgs().SetFileStorageTierAsync(default!, default!, default!);

    private async Task InvokeHeadingMethod(string name, params object[] arguments)
    {
        var method = typeof(StorageHeading).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
        await (Task)method.Invoke(_heading, arguments)!;
    }

    private static void SetPrivateMember(object instance, string name, object value)
    {
        var type = typeof(StorageHeading);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var field = type.GetField(name, flags);
        if (field is not null)
        {
            field.SetValue(instance, value);
            return;
        }

        type.GetProperty(name, flags)!.SetValue(instance, value);
    }
}
