using Bunit;
using Datahub.Core.Components.AuthViews;
using Datahub.Core.Storage;
using Datahub.Portal.Pages.Workspace.Storage;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class FileMetadataEditorSteps : BunitTestSteps
{
    private const string ContainerName = "container";
    private const string FileName = "folder/file.csv";

    private readonly ICloudStorageManager _storageManager = Substitute.For<ICloudStorageManager>();
    private IRenderedComponent<FileMetadataEditor>? _component;
    private IDictionary<string, string> _metadata = new Dictionary<string, string>();

    public FileMetadataEditorSteps()
    {
        ComponentFactories.AddStub<DatahubAuthView>();
        JSInterop.SetupMudBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(Substitute.For<ISnackbar>());
        Services.AddSingleton(Substitute.For<ILogger<FileMetadataEditor>>());

        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()].Returns(call =>
            new LocalizedString(call.Arg<string>(), call.Arg<string>()));
        Services.AddSingleton(localizer);

        _storageManager.GetFileStorageTierAsync(ContainerName, FileName).Returns("Hot");
    }

    [Given("the file has the following metadata")]
    public void GivenTheFileHasTheFollowingMetadata(Table table)
    {
        _metadata = table.Rows.ToDictionary(row => row["Key"], row => row["Value"]);
        _storageManager.GetFileMetadataAsync(ContainerName, FileName).Returns(_metadata);
    }

    [Given("saving metadata fails with {string}")]
    public void GivenSavingMetadataFailsWith(string message)
    {
        _storageManager
            .SetFileMetadataAsync(ContainerName, FileName, Arg.Any<Dictionary<string, string>>())
            .Returns<Task>(_ => throw new InvalidOperationException(message));
    }

    [Given("metadata operations are unsupported")]
    public void GivenMetadataOperationsAreUnsupported()
    {
        _storageManager
            .GetFileMetadataAsync(ContainerName, FileName)
            .Returns<Task<IDictionary<string, string>>>(_ => throw new NotImplementedException());
        _storageManager
            .GetFileStorageTierAsync(ContainerName, FileName)
            .Returns<Task<string>>(_ => throw new NotImplementedException());
    }

    [When("the file metadata editor is rendered for editing")]
    public void WhenTheFileMetadataEditorIsRenderedForEditing()
    {
        _component = Render<FileMetadataEditor>(parameters => parameters
            .Add(component => component.EditMode, true)
            .Add(component => component.ContainerName, ContainerName)
            .Add(component => component.FileName, FileName)
            .Add(component => component.ProjectAcronym, "TEST")
            .Add(component => component.StorageManager, _storageManager));
    }

    [When(@"^the user adds a metadata entry$")]
    public void WhenTheUserAddsAMetadataEntry() => FindButton("Add key").Click();

    [When(@"^the user removes the first metadata entry$")]
    public void WhenTheUserRemovesTheFirstMetadataEntry()
    {
        _component!.FindAll(".metadata-entries button").First().Click();
    }

    [When("the user saves the metadata")]
    public void WhenTheUserSavesTheMetadata() => FindButton("Save").Click();

    [When("the user changes the first metadata value to {string}")]
    public void WhenTheUserChangesTheFirstMetadataValueTo(string value)
    {
        _component!.FindAll("input")[1].Change(value);
    }

    [When("the user cancels metadata editing")]
    public void WhenTheUserCancelsMetadataEditing() => FindButton("Cancel").Click();

    [Then("the metadata editor should display {string}")]
    public void ThenTheMetadataEditorShouldDisplay(string text)
    {
        _component!.Markup.Should().Contain(text);
    }

    [Then("the metadata editor should not display {string}")]
    public void ThenTheMetadataEditorShouldNotDisplay(string text)
    {
        _component!.Markup.Should().NotContain(text);
    }

    [Then(@"^the metadata editor should have two editable entries$")]
    public void ThenTheMetadataEditorShouldHaveTwoEditableEntries()
    {
        AssertEditableEntryCount(2);
    }

    [Then(@"^the metadata editor should have one editable entry$")]
    public void ThenTheMetadataEditorShouldHaveOneEditableEntry()
    {
        AssertEditableEntryCount(1);
    }

    [Then("the metadata should not be saved")]
    public async Task ThenTheMetadataShouldNotBeSaved()
    {
        await _storageManager.DidNotReceiveWithAnyArgs()
            .SetFileMetadataAsync(default!, default!, default!);
    }

    [Then("the file metadata should be saved")]
    public async Task ThenTheFileMetadataShouldBeSaved(Table table)
    {
        var expected = table.Rows.ToDictionary(row => row["Key"], row => row["Value"]);
        await _storageManager.Received(1).SetFileMetadataAsync(
            ContainerName,
            FileName,
            Arg.Is<Dictionary<string, string>>(actual => MetadataMatches(actual, expected)));
    }

    [Then("the metadata editor should leave edit mode")]
    public void ThenTheMetadataEditorShouldLeaveEditMode()
    {
        _component!.Instance.EditMode.Should().BeFalse();
    }

    [Then("the metadata editor should remain in edit mode")]
    public void ThenTheMetadataEditorShouldRemainInEditMode()
    {
        _component!.Instance.EditMode.Should().BeTrue();
    }

    [Then("the metadata should have been loaded {int} times")]
    public async Task ThenTheMetadataShouldHaveBeenLoadedTimes(int count)
    {
        await _storageManager.Received(count).GetFileMetadataAsync(ContainerName, FileName);
    }

    private AngleSharp.Dom.IElement FindButton(string text)
    {
        return _component!.FindAll("button, a")
            .Single(element => element.TextContent.Contains(text, StringComparison.Ordinal));
    }

    private void AssertEditableEntryCount(int count)
    {
        _component!.FindAll(".metadata-entries input").Should().HaveCount(count * 2);
    }

    private static bool MetadataMatches(
        IReadOnlyDictionary<string, string> actual,
        IReadOnlyDictionary<string, string> expected)
    {
        return actual.Count == expected.Count &&
            expected.All(entry =>
                actual.TryGetValue(entry.Key, out var value) && value == entry.Value);
    }
}
