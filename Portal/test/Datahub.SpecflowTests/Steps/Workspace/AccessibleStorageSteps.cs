using Bunit;
using Bunit.TestDoubles;
using Datahub.Application.Services.Security;
using Datahub.Core.Data;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Services.Docs;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Portal.Pages.Workspace.Storage.Container;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;
using static Datahub.Infrastructure.Services.Storage.CloudStorageHelpers;

namespace Datahub.SpecflowTests.Steps.Workspace
{
    [Binding]
    public class AccessibleStorageSteps : BunitTestSteps
    {
        private IRenderedComponent<StorageContainerSelector>? _selector;
        private GcdsDetails? _expandedDisclosure;
        private IRenderedComponent<StorageConfigurationForm>? _form;
        private CloudStorageContainer _selected = null!;
        private ProjectCloudStorage _original = null!;
        private readonly List<CloudStorageContainer> _containers = [];
        private int _saveCount;
        private int? _editedId;
        private bool _cancelled;
        private bool _failSave;
        private TaskCompletionSource? _pendingSave;
        private IRenderedComponent<StorageAccountRemovalForm>? _removal;
        private int _removeCount;
        private bool _failRemoval;
        private TaskCompletionSource? _pendingRemoval;
        private CloudStorageContainer? _reloadedSelection;
        private readonly Dictionary<string, CloudStorageContainer?> _expectedSelections = [];

        private void ConfigureServices()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddMudServices();
            Services.AddLogging();
            var localizer = Substitute.For<IStringLocalizer>();
            localizer[Arg.Any<string>()].Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
            localizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(call => new LocalizedString(call.ArgAt<string>(0), string.Format(call.ArgAt<string>(0), call.ArgAt<object[]>(1))));
            localizer["RemoveStorageNamePrompt", Arg.Any<object[]>()].Returns(call => new LocalizedString("RemoveStorageNamePrompt", string.Format("Remove storage account {0}?", call.ArgAt<object[]>(1))));
            Services.AddSingleton(localizer);
        }

        [Given("the accessible storage selector is rendered for a (.*)")]
        [Given("the accessible storage selector is rendered for an (.*)")]
        public void RenderSelector(string role)
        {
            ConfigureServices();
            var auth = this.AddAuthorization();
            auth.SetAuthorized("Storage user");
            auth.SetRoles($"TEST{(role == "administrator" ? RoleConstants.ADMIN_SUFFIX : RoleConstants.COLLABORATOR_SUFFIX)}");
            var manager = Substitute.For<ICloudStorageManager>();
            _containers.AddRange([
                new("Same name", "first", CloudStorageProviderType.Azure, manager, 1),
                new("Same name", "second", CloudStorageProviderType.Azure, manager, 2),
                new("Same name", "third", CloudStorageProviderType.Azure, manager, 2),
                new("Disabled account", "---", CloudStorageProviderType.AWS, manager, 3, false)
            ]);
            _selected = _containers[0];
            _selector = Render<StorageContainerSelector>(parameters => parameters
                .AddCascadingValue("ProjectAcronym", "TEST")
                .Add(component => component.CloudContainers, _containers)
                .Add(component => component.SelectedContainer, _selected)
                .Add(component => component.AllowBringYourOwn, true)
                .Add(component => component.SelectedContainerChanged, SelectContainer)
                .Add(component => component.OnEditProviderClicked, id => _editedId = id));
        }

        private void SelectContainer(CloudStorageContainer container)
        {
            _selected = container;
            _selector!.Render(parameters => parameters.Add(component => component.SelectedContainer, container));
        }

        [Then("the storage selector displays the current provider account and container")]
        public void CurrentLabel()
        {
            _selector!.FindComponent<GcdsDetails>().Instance.DetailsTitle.Should().Contain("Azure").And.Contain("Same name").And.Contain("first");
            _selector.FindAll("gcds-select").Should().HaveCount(2);
            _selector.FindAll(".mud-menu").Should().BeEmpty();
        }

        [Then("the storage disclosure starts collapsed")]
        public void DisclosureCollapsed()
        {
            _selector!.FindComponent<GcdsDetails>().Instance.Open.Should().NotBe(true);
            _selector.Find("gcds-details").HasAttribute("open").Should().BeFalse();
        }

        [Then("the storage disclosure summary displays the current selection")]
        public void DisclosureSummary() => _selector!.FindComponent<GcdsDetails>().Instance.DetailsTitle.Should().Be(
            $"Current Container: {_selected.CloudStorageProvider} \u2014 {_selected.AccountName} \u2014 {_selected.ContainerName}");

        [Then("storage selection announcements are visually hidden")]
        public void HiddenAnnouncements()
        {
            _selector!.Find("gcds-sr-only[role='status']").TextContent.Should().Contain(_selected.ContainerName);
            _selector.FindAll("gcds-text").Should().BeEmpty();
        }

        [Then("storage controls are inside the disclosure")]
        public void DisclosureControls()
        {
            _selector!.FindAll("gcds-details gcds-select").Should().HaveCount(2);
            _selector.FindAll("gcds-details gcds-button").Should().HaveCount(3);
        }

        [When("I expand the storage disclosure")]
        public void ExpandDisclosure()
        {
            // The web component reflects its browser-owned state to this attribute.
            _expandedDisclosure = _selector!.FindComponent<GcdsDetails>().Instance;
            _selector!.Find("gcds-details").SetAttribute("open", "");
        }

        [Then("the storage disclosure remains expanded")]
        public void DisclosureExpanded()
        {
            // bUnit rebuilds DOM snapshots and cannot retain a browser-mutated attribute.
            // Preserve the component instance and leave Open unset so the browser retains its state.
            var disclosure = _selector!.FindComponent<GcdsDetails>().Instance;
            disclosure.Should().BeSameAs(_expandedDisclosure);
            disclosure.Open.Should().BeNull();
        }

        [When("the inline storage configuration form is open")]
        public void ConfigurationOpen() => _selector!.Render(parameters => parameters
            .Add(component => component.FormOpen, true)
            .Add(component => component.Disabled, true));

        [Then("storage controls are disabled during the inline form")]
        public void DisabledDuringForm()
        {
            _selector!.FindComponents<GcdsSelect>().Should().OnlyContain(component => component.Instance.Disabled);
            _selector.FindComponents<GcdsButton>().Should().OnlyContain(component => component.Instance.Disabled);
        }

        [Then("the storage accounts with the same name remain distinct")]
        public void DistinctAccounts() => _selector!.FindAll("[select-id='storage-account-select'] option").Select(option => option.GetAttribute("value")).Should().OnlyHaveUniqueItems().And.HaveCount(3);

        [When("I select the second external storage account")]
        public Task SelectSecondAccount() => _selector!.InvokeAsync(() => _selector.FindComponents<GcdsSelect>()[0].Instance.ValueChanged.InvokeAsync("Azure:external:2"));

        [Then("its first enabled container becomes selected")]
        public void FirstEnabledSelected() => _selected.Should().BeSameAs(_containers[1]);

        [When("I select another container in that storage account")]
        public Task SelectThirdContainer() => _selector!.InvokeAsync(() => _selector.FindComponents<GcdsSelect>()[1].Instance.ValueChanged.InvokeAsync("third"));

        [Then("that container becomes selected")]
        public void ThirdSelected() => _selected.Should().BeSameAs(_containers[2]);

        [When("I select the disabled storage account")]
        public Task SelectDisabledAccount() => _selector!.InvokeAsync(() => _selector.FindComponents<GcdsSelect>()[0].Instance.ValueChanged.InvokeAsync("AWS:external:3"));

        [Then("the active storage container remains unchanged")]
        public void SelectionUnchanged()
        {
            _selected.Should().BeSameAs(_containers[0]);
            _selector!.FindComponents<GcdsSelect>()[1].Instance.Disabled.Should().BeTrue();
        }

        [Then("storage management targets the disabled account")]
        public async Task EditDisabledAccount()
        {
            await _selector!.InvokeAsync(() => _selector.FindComponents<GcdsButton>().Single(button => button.Markup.Contains("Edit account")).Instance.OnClick.InvokeAsync());
            _editedId.Should().Be(3);
        }

        [Given("external storage is (.*)")]
        public void ExternalStorageAvailability(string availability) => _selector!.Render(parameters => parameters.Add(component => component.AllowBringYourOwn, availability == "allowed"));

        [Then("the add storage action is (.*)")]
        public void AddAvailability(string visibility) => _selector!.FindAll("[button-id='storage-add-button']").Count.Should().Be(visibility == "shown" ? 1 : 0);

        [Then("the edit storage action is (.*)")]
        public void EditAvailability(string visibility) => _selector!.FindAll("[button-id='storage-edit-button']").Count.Should().Be(visibility == "shown" ? 1 : 0);

        [Given("the accessible new storage form is rendered")]
        public void RenderNewForm() => RenderForm(false);

        [Given("the accessible existing storage form is rendered")]
        public void RenderExistingForm() => RenderForm(true);

        private void RenderForm(bool existing)
        {
            ConfigureServices();
            var vault = Substitute.For<IKeyVaultUserService>();
            var data = CloudStorageManagerFactory.CreateNewStorageProperties();
            data[AZ_AccountName] = "testaccount";
            data[AZ_AccountKey] = "a2V5";
            vault.GetAllSecrets(Arg.Any<ProjectCloudStorage>(), "TEST").Returns(data);
            Services.AddSingleton(new CloudStorageManagerFactory(NullLoggerFactory.Instance, vault));
            Services.AddSingleton(new DocumentationService(new ConfigurationBuilder().Build(), NullLogger<DocumentationService>.Instance,
                Substitute.For<IHttpClientFactory>(), Substitute.For<IWebHostEnvironment>(), new MemoryCache(new MemoryCacheOptions())));
            Services.AddSingleton(new MicrosoftIdentityConsentAndConditionalAccessHandler(Substitute.For<IServiceProvider>()));
            _original = new ProjectCloudStorage { Id = existing ? 1 : 0, Provider = "Azure", Name = "Original name", Enabled = true };
            _form = Render<StorageConfigurationForm>(parameters => parameters
                .Add(component => component.ProjectCloudStorage, _original)
                .Add(component => component.WorkspaceAcronym, "TEST")
                .Add(component => component.CloudProvider, CloudStorageProviderType.Azure)
                .Add(component => component.OnCancelled, () => _cancelled = true)
                .Add(component => component.OnSaved, SaveAsync));
        }

        private async Task SaveAsync((ProjectCloudStorage Settings, IDictionary<string, string> ConnectionData) result)
        {
            _saveCount++;
            if (_failSave) throw new InvalidOperationException("Persistence unavailable");
            if (_pendingSave is not null) await _pendingSave.Task;
        }

        [Then("the storage configuration is a labelled inline region")]
        public void InlineForm()
        {
            _form!.Find("section[role='region']").GetAttribute("aria-labelledby").Should().Be("storage-configuration-heading");
            _form.FindAll(".mud-dialog, [role='dialog']").Should().BeEmpty();
        }

        [Then("storage testing requires valid credentials")]
        public void TestDisabled() => Button("Test Connection").Instance.Disabled.Should().BeTrue();

        [When("I choose (.*) in the storage form")]
        public async Task ChangeProvider(string provider)
        {
            var select = _form!.FindComponent<GcdsSelect>();
            await _form.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(provider));
            Button("Test Connection").Instance.Disabled.Should().BeTrue();
        }

        [Then("the AWS credential inputs are shown with masked secrets")]
        public void AwsInputs()
        {
            _form!.FindComponent<AWSConnectionDataInput>().Should().NotBeNull();
            _form.FindComponents<GcdsInput>().Count(input => input.Instance.Type == GcdsInputType.Password).Should().Be(2);
            _form.FindAll("gcds-input").Should().HaveCount(5);
        }

        [Then("the GCP credentials use a multiline input")]
        public void GcpInputs() => _form!.FindComponent<GcdsTextarea>().Instance.Label.Should().Be("Service Account Credentials (JSON)");

        [Then("the storage provider cannot be changed")]
        public void FixedProvider() => _form!.FindComponents<GcdsSelect>().Should().BeEmpty();

        [When("I change the Azure storage credentials")]
        public Task ChangeCredentials() => _form!.InvokeAsync(() => _form.FindComponents<GcdsInput>().Single(input => input.Instance.Id == "storage-azure-accountkey").Instance.ValueChanged.InvokeAsync("invalid-key"));

        [Then("the connection must be tested again before saving")]
        public void VerificationReset()
        {
            _form!.FindComponents<GcdsButton>().Should().NotContain(button => button.Find("gcds-button").TextContent.Trim() == "Save");
            Button("Test Connection").Instance.Disabled.Should().BeFalse();
        }

        [When("I change the storage friendly name")]
        public Task ChangeName() => _form!.InvokeAsync(() => _form.FindComponents<GcdsInput>().Single(input => input.Instance.Id == "storage-friendly-name").Instance.ValueChanged.InvokeAsync("Draft name"));

        [When("I cancel the storage form")]
        public Task CancelForm() => _form!.InvokeAsync(() => Button("Cancel").Instance.OnClick.InvokeAsync());

        [Then("the original storage settings are unchanged")]
        public void DraftDiscarded()
        {
            _cancelled.Should().BeTrue();
            _original.Name.Should().Be("Original name");
        }

        [Then("no storage save callback has run")]
        public void NoSave() => _saveCount.Should().Be(0);

        [When("I clear the unclassified data confirmation")]
        public Task ClearConfirmation() => _form!.InvokeAsync(() => _form.FindComponents<GcdsCheckboxes>().Single(input => input.Instance.Name == "storage-classification").Instance.ValueChanged.InvokeAsync([]));

        [Then("storage saving is disabled")]
        public void SaveDisabled() => Button("Save").Instance.Disabled.Should().BeTrue();

        [When("I test the invalid storage connection")]
        public Task TestConnection() => _form!.InvokeAsync(() => Button("Test Connection").Instance.OnClick.InvokeAsync());

        [Then("the storage connection error appears inline")]
        public void ConnectionError() => _form!.Find("[role='alert']").TextContent.Should().Contain("Unable to successfully connect");

        [Given("storage persistence will fail")]
        public void FailSave() => _failSave = true;

        [When("I save the storage form")]
        public Task SaveForm() => _form!.InvokeAsync(() => Button("Save").Instance.OnClick.InvokeAsync());

        [Then("the storage persistence error appears inline")]
        public void PersistenceError() => _form!.Find("[role='alert']").TextContent.Should().Contain("Unable to save storage configuration");

        [Then("the storage form remains available for retry")]
        public void CanRetry() => Button("Save").Instance.Disabled.Should().BeFalse();

        [Given("storage persistence is pending")]
        public void PendingSave() => _pendingSave = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        [When("I submit the storage form twice")]
        public async Task SubmitTwice()
        {
            var firstSave = SaveForm();
            _form!.WaitForAssertion(() => Button("Save").Instance.Disabled.Should().BeTrue());
            await SaveForm();
            _pendingSave!.SetResult();
            await firstSave;
        }

        [Then("only one storage save callback runs")]
        public void OneSave() => _saveCount.Should().Be(1);

        [Given("the accessible storage removal form is rendered")]
        public void RenderRemoval()
        {
            ConfigureServices();
            Services.AddSingleton(new MicrosoftIdentityConsentAndConditionalAccessHandler(Substitute.For<IServiceProvider>()));
            _removal = Render<StorageAccountRemovalForm>(parameters => parameters
                .Add(component => component.AccountName, "External test account")
                .Add(component => component.OnConfirmed, RemoveAsync)
                .Add(component => component.OnCancelled, () => _cancelled = true));
        }

        private async Task RemoveAsync()
        {
            _removeCount++;
            if (_failRemoval) throw new InvalidOperationException("Key Vault unavailable");
            if (_pendingRemoval is not null) await _pendingRemoval.Task;
        }

        [Then("the removal confirmation names the storage account")]
        public void RemovalNamesAccount()
        {
            _removal!.Find("section").GetAttribute("aria-labelledby").Should().Be("storage-removal-heading");
            _removal.FindAll(".mud-dialog, [role='dialog']").Should().BeEmpty();
            _removal.Find("section").TextContent.Should().Contain("External test account");
        }

        [When("I cancel the storage removal")]
        public Task CancelRemoval() => _removal!.InvokeAsync(() => RemovalButton("Cancel").Instance.OnClick.InvokeAsync());

        [Then("no storage removal callback has run")]
        public void NoRemoval()
        {
            _cancelled.Should().BeTrue();
            _removeCount.Should().Be(0);
        }

        [Given("storage removal is pending")]
        public void PendingRemoval() => _pendingRemoval = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        [Given("storage removal will fail")]
        public void FailRemoval() => _failRemoval = true;

        [When("I confirm storage removal")]
        public Task ConfirmRemoval() => _removal!.InvokeAsync(() => RemovalButton("Confirm removal").Instance.OnClick.InvokeAsync());

        [When("I confirm storage removal twice")]
        public async Task ConfirmTwice()
        {
            var first = ConfirmRemoval();
            _removal!.WaitForAssertion(() => RemovalButton("Confirm removal").Instance.Disabled.Should().BeTrue());
            await ConfirmRemoval();
            _pendingRemoval!.SetResult();
            await first;
        }

        [Then("only one storage removal callback runs")]
        public void OneRemoval() => _removeCount.Should().Be(1);

        [Then("the storage removal error appears inline")]
        public void RemovalError() => _removal!.Find("[role='alert']").TextContent.Should().Contain("Unable to remove storage account");

        [Then("the storage removal can be retried")]
        public void RetryRemoval() => RemovalButton("Confirm removal").Instance.Disabled.Should().BeFalse();

        [Given("storage containers were reloaded with the previous container (.*) and workspace default (.*)")]
        public void ReloadSelection(string previousState, string defaultState)
        {
            var manager = Substitute.For<ICloudStorageManager>();
            var previous = new CloudStorageContainer("external", "previous", CloudStorageProviderType.Azure, manager, 1);
            var reloadedPrevious = new CloudStorageContainer("renamed external", "previous", CloudStorageProviderType.Azure, manager, 1, previousState == "enabled");
            var workspaceDefault = new CloudStorageContainer("workspace", "default", CloudStorageProviderType.Azure, manager);
            var first = new CloudStorageContainer("workspace", "first", CloudStorageProviderType.Azure, manager);
            List<CloudStorageContainer> reloaded = defaultState == "empty" ? [] : [first];
            if (previousState != "removed") reloaded.Add(reloadedPrevious);
            if (defaultState == "enabled") reloaded.Add(workspaceDefault);
            _reloadedSelection = StorageContainerSelection.SelectAfterReload(reloaded, previous, workspaceDefault);
            _expectedSelections["previous"] = reloadedPrevious;
            _expectedSelections["default"] = workspaceDefault;
            _expectedSelections["first"] = first;
            _expectedSelections["none"] = null;
        }

        [Then("the reloaded selection is (.*)")]
        public void ReloadedSelection(string selection) => _reloadedSelection.Should().BeSameAs(_expectedSelections[selection]);

        private IRenderedComponent<GcdsButton> RemovalButton(string text) => _removal!.FindComponents<GcdsButton>().Single(button => button.Find("gcds-button").TextContent.Trim() == text);

        private IRenderedComponent<GcdsButton> Button(string text) => _form!.FindComponents<GcdsButton>().Single(button => button.Find("gcds-button").TextContent.Trim() == text);
    }
}
