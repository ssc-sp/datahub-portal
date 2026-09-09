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
    private bool _archiveConfirmation = true;

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
    public void GivenFilePropertiesWithTier(string tier) => _itemTier = tier;

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
    }

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
