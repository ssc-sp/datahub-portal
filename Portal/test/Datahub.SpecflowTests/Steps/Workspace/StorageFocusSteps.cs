using System.Reflection;
using Bunit;
using Bunit.TestDoubles;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.Docs;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Portal.Components;
using Datahub.Portal.Pages.Workspace.Storage;
using Datahub.Portal.Pages.Workspace.Storage.Container;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using Microsoft.JSInterop;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace
{
    [Binding]
    public class StorageFocusSteps : BunitTestSteps
    {
        private const string ModulePath = "./Pages/Workspace/Storage/FileExplorerPage.razor.js";
        private IRenderedComponent<LoadedStorageExplorer> _page = null!;
        private BunitJSInterop _module = null!;
        private int _focusCount;
        private string _action = string.Empty;

        [Given("the storage explorer is ready for an administrator")]
        public void RenderExplorer()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
            _module = JSInterop.SetupModule(ModulePath);
            _module.Mode = JSRuntimeMode.Loose;
            Services.AddMudServices();
            Services.AddLogging();
            var auth = this.AddAuthorization();
            auth.SetAuthorized("Storage administrator");
            auth.SetRoles($"TEST{RoleConstants.ADMIN_SUFFIX}");

            var localizer = Substitute.For<IStringLocalizer>();
            localizer[Arg.Any<string>()].Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
            localizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(call => new LocalizedString(call.ArgAt<string>(0), string.Format(call.ArgAt<string>(0), call.ArgAt<object[]>(1))));
            Services.AddSingleton(localizer);
            Services.AddSingleton(Substitute.For<IUserInformationService>());
            Services.AddSingleton(Substitute.For<IUserTokenCredentialService>());
            Services.AddSingleton(Substitute.For<IProjectStorageConfigurationService>());
            Services.AddSingleton(Substitute.For<IFeatureManagerSnapshot>());
            Services.AddSingleton(new DatahubPortalConfiguration());
            Services.AddSingleton(new MicrosoftIdentityConsentAndConditionalAccessHandler(Substitute.For<IServiceProvider>()));
            var vault = Substitute.For<IKeyVaultUserService>();
            vault.GetAllSecrets(Arg.Any<ProjectCloudStorage>(), "TEST").Returns(CloudStorageManagerFactory.CreateNewStorageProperties());
            Services.AddSingleton(vault);
            Services.AddSingleton(new CloudStorageManagerFactory(NullLoggerFactory.Instance, vault));
            Services.AddSingleton(new DocumentationService(new ConfigurationBuilder().Build(), NullLogger<DocumentationService>.Instance,
                Substitute.For<IHttpClientFactory>(), Substitute.For<IWebHostEnvironment>(), new MemoryCache(new MemoryCacheOptions())));

            var options = new DbContextOptionsBuilder<DatahubProjectDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using (var context = new DatahubProjectDBContext(options))
            {
                context.ProjectCloudStorages.Add(new ProjectCloudStorage { Id = 1, ProjectId = 7, Provider = "Azure", Name = "External account", Enabled = true });
                context.SaveChanges();
            }
            Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(new SpecFlowDbContextFactory(options));
            // Keep the real selector, inline forms, page callbacks, and render lifecycle.
            ComponentFactories.AddStub<DHMainContentTitle>();
            ComponentFactories.AddStub<WorkspaceAlerts>();
            ComponentFactories.AddStub<FileExplorer>();
            _page = Render<LoadedStorageExplorer>(parameters => parameters.Add(page => page.WorkspaceAcronym, "TEST"));
        }

        [Then("the storage explorer has not requested focus")]
        public void NoFocus() => _module.Invocations.Where(call => call.Identifier == "focusStorageElement").Should().BeEmpty();

        [When("I open the storage explorer (.*) form")]
        public Task OpenForm(string action)
        {
            _action = action;
            var selector = _page.FindComponent<StorageContainerSelector>().Instance;
            return _page.InvokeAsync(() => action switch
            {
                "add" => selector.OnAddProviderClicked.InvokeAsync(CloudStorageProviderType.Azure),
                "edit" => selector.OnEditProviderClicked.InvokeAsync(1),
                "remove" => selector.OnRemoveProviderClicked.InvokeAsync(1),
                _ => throw new ArgumentException("Unknown storage action", nameof(action))
            });
        }

        [Then("the storage explorer requests focus on {string}")]
        public void FocusTarget(string target)
        {
            _page.WaitForAssertion(() => _module.Invocations.Last(call => call.Identifier == "focusStorageElement").Arguments.Should().Equal(target));
            _focusCount = _module.Invocations.Count(call => call.Identifier == "focusStorageElement");
        }

        [Then("the active storage explorer form is outside the disclosure")]
        public void FormOutsideDisclosure()
        {
            var id = _action == "remove" ? "storage-removal-confirmation" : "storage-configuration-form";
            _page.Find($"#{id}").Closest("gcds-details").Should().BeNull();
        }

        [When("I cancel the active storage explorer form")]
        public Task CancelForm() => _page.InvokeAsync(() => _action == "remove"
            ? _page.FindComponent<StorageAccountRemovalForm>().Instance.OnCancelled.InvokeAsync()
            : _page.FindComponent<StorageConfigurationForm>().Instance.OnCancelled.InvokeAsync());

        [Then("the storage focus module is imported once")]
        public void ModuleImportedOnce() => JSInterop.Invocations.Count(call => call.Identifier == "import" && call.Arguments.Contains(ModulePath)).Should().Be(1);

        [When("the storage explorer renders again")]
        public void RenderAgain() => _page.Render();

        [Then("the storage focus request is not repeated")]
        public void NoRepeatedFocus() => _module.Invocations.Count(call => call.Identifier == "focusStorageElement").Should().Be(_focusCount);

        [Given("the storage focus JavaScript connection is disconnected")]
        public void DisconnectFocus() => _module.SetupVoid("focusStorageElement", _ => true)
            .SetException(new JSDisconnectedException("Test connection disconnected"));

        public class LoadedStorageExplorer : FileExplorerPage
        {
            protected override Task OnInitializedAsync()
            {
                // Seed an already-loaded page without contacting cloud storage during these component tests.
                // Reflection confines private page-state setup to this test fixture.
                var manager = Substitute.For<ICloudStorageManager>();
                var container = new CloudStorageContainer("External account", "files", CloudStorageProviderType.Azure, manager, 1);
                SetPageField("_loading", false);
                SetPageField("_isProjectKeyEnabled", true);
                SetPageField("_isUserProjectAdmin", true);
                SetPageField("_projectId", 7);
                SetPageField("_project", new Datahub_Project { Project_ID = 7, Project_Acronym_CD = "TEST" });
                SetPageField("_user", new PortalUser { Id = 1, Email = "admin@example.gc.ca" });
                SetPageField("_cloudContainers", new List<CloudStorageContainer> { container });
                SetPageField("_selectedContainer", container);
                SetPageField("_workspaceContainer", container);
                return Task.CompletedTask;
            }

            private void SetPageField(string name, object value) => typeof(FileExplorerPage)
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(this, value);
        }
    }
}
