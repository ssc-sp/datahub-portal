using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Bunit;
using Bunit.TestDoubles;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Toolbox;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Toolbox;
using Datahub.Portal.Pages.Workspace.Toolbox;
using Datahub.Shared;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.Security;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using NSubstitute.Extensions;
using Reqnroll;
using ResourceMessagingService = Datahub.Infrastructure.Services.ResourceMessagingService;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class WorkspaceToolboxSteps(
    ScenarioContext scenarioContext,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    DatahubPortalConfiguration datahubPortalConfiguration) : TestContext
{
    private const string RelativePathToSrc = "../../../../../src";


    [Given(@"the user is on the workspace toolbox page")]
    public void GivenTheUserIsOnTheWorkspaceToolboxPage()
    {
        AddRequiredServices();
        SetupJS();

        var workspaceToolbox = RenderComponent<CascadingAuthenticationState>(parameters =>
        {
            parameters.AddChildContent<MudPopoverProvider>();
            parameters.AddChildContent<MudDialogProvider>();
            parameters.AddChildContent<WorkspaceToolboxPage>(childParameters =>
            {
                childParameters.Add(p => p.WorkspaceAcronym, Testing.WorkspaceAcronym);
                childParameters.Add(p => p.ElevatedWorkspaceAccessEnabled, true);
            });
        });

        scenarioContext.Add("workspaceToolbox", workspaceToolbox);
    }

    private void SetupJS()
    {
        var module = JSInterop.SetupModule("./_content/Datahub.Core/Components/SkipLink.razor.js");
        JSInterop.Setup<BunitJSInterop>("import", "./_content/Datahub.Core/Components/SkipLink.razor.js")
            .SetResult(module);
        module.SetupVoid("focusElement", Arg.Any<string>());
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private void AddRequiredServices()
    {
        var portalConfiguration = new DatahubPortalConfiguration()
        {
            CultureSettings =
            {
                ResourcesPath = $"{RelativePathToSrc}/Datahub.Portal/i18n",
                AdditionalResourcePaths = []
            }
        };
        var toolboxService = new ToolboxService();
        Services.AddSingleton<IToolboxService>(toolboxService);
        var dialogService = new DialogService();
        Services.AddSingleton<IDialogService>(dialogService);
        Services.AddSingleton(datahubPortalConfiguration);
        Services.AddStub<IDatahubAuditingService>();

        var requestLogger = new Logger<RequestManagementService>(new LoggerFactory());

        var endpointProvider = Substitute.For<ISendEndpointProvider>();
        var resourceMessagingService = new ResourceMessagingService(dbContextFactory, endpointProvider);
        var requestManagementService = new RequestManagementService(
            requestLogger,
            dbContextFactory,
            Substitute.For<IDatahubAuditingService>(),
            resourceMessagingService
        );
        Services.AddSingleton<IRequestManagementService>(requestManagementService);
        Services.AddStub<IWebHostEnvironment>();
        Services.AddSingleton<IResourceMessagingService>(resourceMessagingService);
        var logger = new Logger<WorkspaceToolboxPage>(new LoggerFactory());
        Services.AddSingleton(logger);
        Services.AddSingleton<NavigationManager>(new FakeNavigationManager(this));
        Services.AddSingleton(dbContextFactory);
        Services.AddDatahubOfflineInfrastructureServices(portalConfiguration);
        var authContext = new TestAuthorizationContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "test@test.com"),
                new Claim(ClaimTypes.Role, RoleConstants.DATAHUB_ROLE_ADMIN)
            }, "TestAuth"))
        };
        Services.AddScoped<IAuthorizationService>(sp =>
            new TestAuthorizationService(authContext));
        Services.AddScoped<AuthenticationStateProvider>(sp =>
            new TestAuthStateProvider(authContext));
        Services.AddSingleton<IAuthorizationPolicyProvider, TestAuthorizationPolicyProvider>();
        Services.AddMudLocalization();
        Services.AddMudServices();
        Services.AddLocalization();
    }

    [Then(@"the user should see the toolbox")]
    public void ThenTheUserShouldSeeTheToolbox()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Find($"#{WorkspaceToolboxPage.CatalogId}").Should().NotBeNull();
    }

    [Given(@"the workspace has the (.*) tool")]
    public void GivenTheWorkspaceHas(string tool)
    {
        var dbContext = dbContextFactory.CreateDbContext();
        var project = dbContext.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
        var resource = new Project_Resources2
        {
            CreatedAt = DateTime.Now,
            ProjectId = project.Project_ID,
            Status = TerraformStatus.Completed,
            ResourceType = TerraformTemplate.GetTerraformServiceType(tool)
        };
        dbContext.Project_Resources2.Add(resource);
        dbContext.SaveChanges();
    }

    [Then(@"(.*) should be in the Existing Tools section")]
    public void ThenShouldBeInTheExistingToolsSection(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var existing = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ExistingId}");
        existing.Should().NotBeNull();
        existing.Children.Where(e =>
                e.Attributes["id"]!.Value ==
                WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.ExistingId, tool]))
            .Should().HaveCount(1);
    }

    [Then(@"(.*) should not be in the Catalog section")]
    public void ThenShouldNotBeInTheCatalogSection(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var catalog = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.CatalogId}");
        catalog.Should().NotBeNull();
        catalog.Children.Where(e =>
                e.Attributes["id"]!.Value ==
                WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CatalogId, tool]))
            .Should().HaveCount(0);
    }

    [Given(@"a workspace with no metadata")]
    public void GivenTheWorkspaceDoesNotHaveMetadata()
    {
        var dbContext = dbContextFactory.CreateDbContext();
        var project = dbContext.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
        project.MetadataAdded = false;
        dbContext.SaveChanges();
    }

    [Then(@"they should not see the toolbox")]
    public void ThenTheyShouldNotSeeTheToolbox()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Invoking(t => t.Find($"#{WorkspaceToolboxPage.CatalogId}"))
            .Should()
            .Throw<ElementNotFoundException>();
    }

    [Then(@"they should instead see a metadata warning")]
    public void ThenTheyShouldInsteadSeeAMetadataWarning()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Find($"#{WorkspaceToolboxPage.MetadataWarningId}").Should().NotBeNull();
    }

    [Given(@"the workspace does not have (.*)")]
    public void GivenTheWorkspaceDoesNotHave(string tool)
    {
        // Workspace does not have any resources by default
    }

    [Then(@"(.*) should be in the Catalog section")]
    public void ThenShouldBeInTheCatalogSection(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var catalog = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.CatalogId}");
        catalog.Should().NotBeNull();
        catalog.Children.Where(e =>
                e.Attributes["id"]!.Value ==
                WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CatalogId, tool]))
            .Should().HaveCount(1);
    }

    [Then(@"(.*) should not be in the Existing Tools section")]
    public void ThenShouldNotBeInTheExistingToolsSection(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        try
        {
            // The below line should fail, since the existing section won't exist when there are no existing tools
            var existing = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ExistingId}");
            // But if it does not fail, then it should not contain the element
            existing.Should().NotBeNull();
            existing.Children.Where(e =>
                    e.Attributes["id"]!.Value ==
                    WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.ExistingId, tool]))
                .Should().HaveCount(0);
        }
        catch (ElementNotFoundException e)
        {
            // Nothing to do here
        }
    }

    [When(@"the user clicks the Add button for (.*), if it is (.*)")]
    public void WhenTheUserClicksTheAddButtonForIfItIs(string tool, bool available)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        if (available)
        {
            var button =
                workspaceToolbox!.Find(
                    $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CatalogId, tool, WorkspaceToolboxPage.AddButtonId])}");
            var catalog = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.CatalogId}");
            scenarioContext["catalogCount"] = catalog.Children.Length;
            button.Click();
        }
        else
        {
            workspaceToolbox!.Invoking(t => t.Find(
                    $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CatalogId, tool, WorkspaceToolboxPage.AddButtonId])}"))
                .Should()
                .Throw<ElementNotFoundException>();
        }

        scenarioContext["available"] = available;
        scenarioContext["tool"] = tool;
    }

    [Then(@"(.*) should be in the Summary section as an added tool")]
    public void ThenShouldBeInTheSummarySectionAsAnAddedTool(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var available = (bool)scenarioContext["available"];
        if (available)
        {
            workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryAddId, tool])}")
                .Should()
                .NotBeNull();
        }
        else
        {
            workspaceToolbox!.Invoking(t =>
                    t.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryAddId, tool])}"))
                .Should()
                .Throw<ElementNotFoundException>();
        }
    }

    [When(@"the user clicks the Remove button for (.*), if it is (.*)")]
    public void WhenTheUserClicksTheRemoveButtonForIfItIs(string tool, bool removable)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var button =
            workspaceToolbox!.Find(
                $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.ExistingId, tool, WorkspaceToolboxPage.RemoveButtonId])}");
        var existing = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ExistingId}");
        scenarioContext["existingCount"] = existing.Children.Length;
        button.Click();

        scenarioContext["removable"] = removable;
    }

    [Then(@"(.*) should be in the Summary section as a removed tool")]
    public void ThenShouldBeInTheSummarySectionAsARemovedTool(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var removable = (bool)scenarioContext["removable"];
        if (removable)
        {
            workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryRemoveId, tool])}")
                .Should()
                .NotBeNull();
        }
        else
        {
            workspaceToolbox!.Invoking(t =>
                    t.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryRemoveId, tool])}"))
                .Should()
                .Throw<ElementNotFoundException>();
        }
    }

    [When(@"the user clicks the Configure button for (.*), if it is (.*)")]
    public void WhenTheUserClicksTheConfigureButtonForIfItIs(string tool, bool configurable)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var button =
            workspaceToolbox!.Find(
                $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.ExistingId, tool, WorkspaceToolboxPage.ConfigureButtonId])}");
        var existing = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ExistingId}");
        scenarioContext["existingCount"] = existing.Children.Length;
        button.Click();
        scenarioContext["tool"] = tool;
        scenarioContext["configurable"] = configurable;
    }

    [Then(@"(.*) should be in the Summary section as a configured tool")]
    public void ThenShouldBeInTheSummarySectionAsAConfiguredTool(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var configurable = (bool)scenarioContext["configurable"];
        if (configurable)
        {
            workspaceToolbox!
                .Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryConfigureId, tool])}")
                .Should()
                .NotBeNull();
        }
        else
        {
            workspaceToolbox!.Invoking(t =>
                    t.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryConfigureId, tool])}"))
                .Should()
                .Throw<ElementNotFoundException>();
        }
    }

    [Then(@"(.*) dependencies for (.*) should be in the Summary section as added tools")]
    public void ThenDependenciesForShouldBeInTheSummarySectionAsAddedTools(int dependencyCount, string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var dependencies = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.SummaryAddId}");
        dependencies.Children.Should().HaveCount(dependencyCount + 1);
        scenarioContext["dependencyCount"] = dependencyCount;
    }

    [Then(@"(.*) and its (.*) dependencies should not be in the Catalog section as available tools")]
    public void ThenAndItsDependenciesShouldNotBeInTheCatalogSectionAsAvailableTools(string tool, int dependencyCount)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var catalogCount = (int)scenarioContext["catalogCount"];
        var expectedCount = catalogCount - dependencyCount - 1;
        var catalog = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.CatalogId}");
        catalog.Children.Should().HaveCount(expectedCount);
    }

    [When(@"the user clicks the Cancel button for an (.*) of (.*)")]
    public void WhenTheUserClicksTheCancelButtonForAnDependencyOf(string exampleDependency, string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var dependencyCancelButton =
            workspaceToolbox!.Find(
                $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryAddId, exampleDependency, WorkspaceToolboxPage.CancelButtonId])}");
        dependencyCancelButton.Click();
    }

    [Then(@"should instead be back in the Catalog section")]
    public void ThenShouldInsteadBeBackInTheCatalogSection()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var catalogCount = (int)scenarioContext["catalogCount"];
        var dependencyCount = (int)scenarioContext["dependencyCount"];
        var expectedCount =
            catalogCount - (dependencyCount - 1);
        var catalog = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.CatalogId}");
        catalog.Children.Should().HaveCount(expectedCount);
    }

    [Then(@"(.*) and the one canceled dependency should not be in the Summary section")]
    public void ThenAndTheOneCanceledDependencyShouldNotBeInTheSummarySection(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var dependencyCount = (int)scenarioContext["dependencyCount"];
        switch (dependencyCount)
        {
            case 1:
                workspaceToolbox!.Invoking(t => t.Find($"#{WorkspaceToolboxPage.SummaryAddId}"))
                    .Should()
                    .Throw<ElementNotFoundException>();
                break;
            case > 1:
                var dependencies = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.SummaryAddId}");
                dependencies.Children.Should().HaveCount(dependencyCount - 1);
                break;
        }
    }

    [Then(@"any additional dependencies should still be in the Summary section")]
    public void ThenAnyAdditionalDependenciesShouldStillBeInTheSummarySection()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var dependencyCount = (int)scenarioContext["dependencyCount"];
        switch (dependencyCount)
        {
            case >= 2
                : // If there are more than 1 dependencies to a tool, the first would be removed and the tool itself would be removed as well, but there would still be remaining dependencies
                var summaryAdd = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.SummaryAddId}");
                summaryAdd.Children.Should().HaveCount(dependencyCount - 1);
                break;
            case < 2: // Otherwise, there would be nothing in the summary left
                workspaceToolbox!.Invoking(t => t.Find($"#{WorkspaceToolboxPage.SummaryAddId}"))
                    .Should()
                    .Throw<ElementNotFoundException>();
                break;
        }
    }

    [When(@"the user clicks the Cancel button for (.*) in the Remove section")]
    public void WhenTheUserClicksTheCancelButtonForInTheRemoveSection(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var removable = (bool)scenarioContext["removable"];
        if (removable)
        {
            var cancelButton =
                workspaceToolbox!.Find(
                    $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryRemoveId, tool, WorkspaceToolboxPage.CancelButtonId])}");
            cancelButton.Click();
        }
    }

    [Then(@"(.*) should not be in the Remove section of the Summary")]
    public void ThenShouldNotBeInTheRemoveSectionOfTheSummary(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Invoking(t =>
                t.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryRemoveId, tool])}"))
            .Should()
            .Throw<ElementNotFoundException>();
    }

    [Then(@"should instead be back in the Existing Tools section")]
    public void ThenShouldInsteadBeBackInTheExistingToolsSection()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var expectedExistingCount = (int)scenarioContext["existingCount"];
        var existing = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ExistingId}");
        existing.Children.Length.Should().Be(expectedExistingCount);
    }

    [When(@"the user clicks the Cancel button for (.*) in the Configure section")]
    public void WhenTheUserClicksTheCancelButtonForInTheConfigureSection(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var configurable = (bool)scenarioContext["configurable"];
        if (configurable)
        {
            var cancelButton =
                workspaceToolbox!.Find(
                    $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryConfigureId, tool, WorkspaceToolboxPage.CancelButtonId])}");
            cancelButton.Click();
        }
    }

    [Then(@"(.*) should not be in the Configure section of the Summary")]
    public void ThenShouldNotBeInTheConfigureSectionOfTheSummary(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Invoking(t =>
                t.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryConfigureId, tool])}"))
            .Should()
            .Throw<ElementNotFoundException>();
    }

    [Given(@"there are no tools being added, removed, or configured")]
    public void GivenThereAreNoToolsBeingAddedRemovedOrConfigured()
    {
        // No tools are being added, removed, or configured by default
    }

    [When(@"the user clicks the Next button")]
    public void WhenTheUserClicksTheNextButton()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var nextButton = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.NextButtonId}");
        nextButton.Click();
    }

    [Then(@"the user should not be able to proceed")]
    public void ThenTheUserShouldNotBeAbleToProceed()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var workspaceToolboxComponent = workspaceToolbox!.FindComponent<WorkspaceToolboxPage>();
        Testing.GetPrivateField(workspaceToolboxComponent, "_activeIndex", out var activeIndex);
        activeIndex.Should().Be(0);
    }

    [Given(@"they have done an (.*) on a (.*)")]
    public void GivenTheyHaveDoneAnOnA(string action, string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        scenarioContext["tool"] = tool;
        scenarioContext["action"] = action;
        switch (action)
        {
            case "added":
                var addButton =
                    workspaceToolbox!.Find(
                        $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CatalogId, tool, WorkspaceToolboxPage.AddButtonId])}");
                addButton.Click();
                break;
            case "removed":
                GivenTheWorkspaceHas(tool);
                workspaceToolbox!.Render();
                var removeButton =
                    workspaceToolbox!.Find(
                        $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.ExistingId, tool, WorkspaceToolboxPage.RemoveButtonId])}");
                removeButton.Click();
                break;
            case "configured":
                GivenTheWorkspaceHas(tool);
                workspaceToolbox!.Render();
                var configureButton =
                    workspaceToolbox!.Find(
                        $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.ExistingId, tool, WorkspaceToolboxPage.ConfigureButtonId])}");
                configureButton.Click();
                break;
        }
    }

    [When(@"the user clicks the Previous button")]
    public void WhenTheUserClicksThePreviousButton()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var previousButton = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.PreviousButtonId}");
        previousButton.Click();
    }

    [Then(@"the selected tool should still be selected")]
    public void ThenTheSelectedToolShouldStillBeSelected()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var tool = (string)scenarioContext["tool"];
        var action = (string)scenarioContext["action"];
        switch (action)
        {
            case "added":
                workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryAddId, tool])}")
                    .Should()
                    .NotBeNull();
                break;
            case "removed":
                workspaceToolbox!
                    .Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryRemoveId, tool])}")
                    .Should()
                    .NotBeNull();
                break;
            case "configured":
                workspaceToolbox!
                    .Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryConfigureId, tool])}")
                    .Should()
                    .NotBeNull();
                break;
        }
    }

    [Given(@"the workspace has (.*) if it is not being added \((.*)\)")]
    public void GivenTheWorkspaceHasIfItIsNotBeingAdded(string tool, string action)
    {
        if (action != "added")
        {
            GivenTheWorkspaceHas(tool);
        }
    }

    [Then(@"they should reach the (.*) step")]
    public void ThenTheyShouldReachTheStep(int index)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var workspaceToolboxComponent = workspaceToolbox!.FindComponent<WorkspaceToolboxPage>();
        Testing.GetPrivateField(workspaceToolboxComponent, "_activeIndex", out var activeIndex);
        activeIndex.Should().Be(index);
    }

    [Then(@"they should be back on the selection step")]
    public void ThenTheyShouldBeBackOnTheSelectionStep()
    {
        ThenTheyShouldReachTheStep(0);
    }

    [Then(@"there should be an underlying Add transaction for (.*)")]
    public void ThenThereShouldBeAnUnderlyingAddTransactionFor(string tool)
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var available = (bool)scenarioContext["available"];
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        if (available)
        {
            Testing.GetPrivateField(workspaceToolbox!, "_transactions", out var transactions);
            (transactions as List<ToolboxTransaction>)!
                .FirstOrDefault(t => t.Tool == tool && t.Type == ToolboxTransactionType.Add)
                .Should()
                .NotBeNull();
        }
    }

    [When(@"the user clicks the Cancel button for (.*) in the Add section")]
    public void WhenTheUserClicksTheCancelButtonFor(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var available = (bool)scenarioContext["available"];
        if (available)
        {
            var cancelButton =
                workspaceToolbox!.Find(
                    $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryAddId, tool, WorkspaceToolboxPage.CancelButtonId])}");
            cancelButton.Click();
        }
    }

    [Then(@"(.*) should not be in the Summary Add section")]
    public void ThenShouldNotBeInTheSummaryAddSection(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Invoking(t =>
                t.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.SummaryAddId, tool])}"))
            .Should()
            .Throw<ElementNotFoundException>();
    }

    [Then(@"(.*) should be back in the Catalog section")]
    public void ThenShouldBeBackInTheCatalogSection(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CatalogId, tool])}")
            .Should()
            .NotBeNull();
    }

    [Then(@"there should be no underlying Add transaction for (.*)")]
    public void ThenThereShouldBeNoUnderlyingAddTransactionFor(string tool)
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        Testing.GetPrivateField(workspaceToolbox!, "_transactions", out var transactions);
        (transactions as List<ToolboxTransaction>)!
            .FirstOrDefault(t => t.Tool == tool && t.Type == ToolboxTransactionType.Add)
            .Should()
            .BeNull();
    }

    [Then(@"there should be an underlying Remove transaction for (.*)")]
    public void ThenThereShouldBeAnUnderlyingRemoveTransactionFor(string tool)
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var removable = (bool)scenarioContext["removable"];
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        if (removable)
        {
            Testing.GetPrivateField(workspaceToolbox!, "_transactions", out var transactions);
            (transactions as List<ToolboxTransaction>)!
                .FirstOrDefault(t => t.Tool == tool && t.Type == ToolboxTransactionType.Remove)
                .Should()
                .NotBeNull();
        }
    }

    [Then(@"there should be no underlying Remove transaction for (.*)")]
    public void ThenThereShouldBeNoUnderlyingRemoveTransactionFor(string tool)
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        Testing.GetPrivateField(workspaceToolbox!, "_transactions", out var transactions);
        (transactions as List<ToolboxTransaction>)!
            .FirstOrDefault(t => t.Tool == tool && t.Type == ToolboxTransactionType.Remove)
            .Should()
            .BeNull();
    }

    [Then(@"there should be an underlying Configure transaction for (.*)")]
    public void ThenThereShouldBeAnUnderlyingConfigureTransactionFor(string tool)
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var configurable = (bool)scenarioContext["configurable"];
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        if (configurable)
        {
            Testing.GetPrivateField(workspaceToolbox!, "_transactions", out var transactions);
            (transactions as List<ToolboxTransaction>)!
                .FirstOrDefault(t => t.Tool == tool && t.Type == ToolboxTransactionType.Update)
                .Should()
                .NotBeNull();
        }
    }

    [Then(@"there should be no underlying Configure transaction for (.*)")]
    public void ThenThereShouldBeNoUnderlyingConfigureTransactionFor(string tool)
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        Testing.GetPrivateField(workspaceToolbox!, "_transactions", out var transactions);
        (transactions as List<ToolboxTransaction>)!
            .FirstOrDefault(t => t.Tool == tool && t.Type == ToolboxTransactionType.Update)
            .Should()
            .BeNull();
    }

    [Then(@"the underlying Configure transaction should have the correct (.*) with the correct (.*) and (.*)")]
    public void ThenTheUnderlyingConfigureTransactionShouldHaveTheCorrectConfiguration(string configType,
        string configParam, string configValue)
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var configurable = (bool)scenarioContext["configurable"];
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        if (configurable)
        {
            var tool = (string)scenarioContext["tool"];
            Testing.GetPrivateField(workspaceToolbox!, "_transactions", out var transactions);
            var transaction = (transactions as List<ToolboxTransaction>)!
                .FirstOrDefault(t => t.Tool == tool && t.Type == ToolboxTransactionType.Update);
            string ogDataType = transaction!.OriginalData!.GetType().ToString();
            string newDataType = transaction!.UpdatedData!.GetType().ToString();
            ogDataType.Should().Be(newDataType);
            ogDataType.Should().Be(configType);
            newDataType.Should().Be(configType);
            if (configValue != "null")
            {
                var ogDataStr = JsonSerializer.Serialize(transaction.OriginalData);
                var ogData = JsonSerializer.Deserialize<JsonObject>(ogDataStr);
                string ogValue = ogData![configParam]?.ToString() ?? string.Empty;
                ogValue.Should().Be(configValue);
            }
        }
    }

    [Given(@"the (.*) has an (.*) value for (.*) \((.*) in db\)")]
    public void GivenTheHasAnValueFor(string tool, string configValue, string configParam, string dbParam)
    {
        var dbContext = dbContextFactory.CreateDbContext();
        var project = dbContext.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
        var resource = dbContext.Project_Resources2.First(p =>
            p.ProjectId == project.Project_ID && p.ResourceType == TerraformTemplate.GetTerraformServiceType(tool));
        var jsonObject = new JsonObject { { dbParam, configValue } };
        resource.InputJsonContent = jsonObject.ToJsonString();
        dbContext.SaveChanges();
    }

    [Then(@"the underlying Add transaction should have the correct (.*) if the tool is (.*)")]
    public void ThenTheUnderlyingAddTransactionShouldHaveTheCorrect(string configType, bool configurable)
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var available = (bool)scenarioContext["available"];
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        if (available && configurable)
        {
            var tool = (string)scenarioContext["tool"];
            Testing.GetPrivateField(workspaceToolbox!, "_transactions", out var transactions);
            var transaction = (transactions as List<ToolboxTransaction>)!
                .FirstOrDefault(t => t.Tool == tool && t.Type == ToolboxTransactionType.Add);
            object? ogData = transaction!.OriginalData;
            ogData.Should().BeNull();
            string newDataType = transaction!.UpdatedData!.GetType().ToString();
            newDataType.Should().Be(configType);
        }
    }

    [Given(@"the workspace has (.*) credits")]
    public void GivenTheWorkspaceHasInvalidCredits(string invalid)
    {
        var dbContext = dbContextFactory.CreateDbContext();
        var project = dbContext.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
        var credits = dbContext.Project_Credits.First(c => c.ProjectId == project.Project_ID);
        switch (invalid)
        {
            case "negative":
                credits.Current = -1;
                break;
            case "null":
                dbContext.Project_Credits.RemoveRange(credits);
                project.Credits = null;
                break;
        }

        dbContext.SaveChanges();
    }

    [Given(@"the workspace has (.*) budget")]
    public void GivenTheWorkspaceHasBudget(string invalid)
    {
        var dbContext = dbContextFactory.CreateDbContext();
        var project = dbContext.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
        switch (invalid)
        {
            case "negative":
                project.Project_Budget = -10.0M;
                break;
            case "null":
                project.Project_Budget = null;
                break;
        }

        dbContext.SaveChanges();
    }

    [Given(@"the workspace has (.*) in (.*)")]
    public void GivenTheWorkspaceHasIn(string tool, string state)
    {
        GivenTheWorkspaceHas(tool);
        var dbContext = dbContextFactory.CreateDbContext();
        var project = dbContext.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
        var resource = dbContext.Project_Resources2.First(p =>
            p.ProjectId == project.Project_ID && p.ResourceType == TerraformTemplate.GetTerraformServiceType(tool));
        switch (state)
        {
            case "completed":
                resource.Status = TerraformStatus.Completed;
                break;
            case "in-progress":
                resource.Status = TerraformStatus.InProgress;
                break;
            case "create-requested":
                resource.Status = TerraformStatus.CreateRequested;
                break;
            case "delete-requested":
                resource.Status = TerraformStatus.DeleteRequested;
                break;
            case "deleted":
                resource.Status = TerraformStatus.Deleted;
                break;
            case "failed":
                resource.Status = TerraformStatus.Failed;
                break;
        }

        dbContext.Project_Resources2.Update(resource);
        dbContext.SaveChanges();
    }

    [Then(@"the (.*) should show the correct (.*) in the toolbox")]
    public void ThenTheShouldShowTheCorrectInTheToolbox(string tool, string state)
    {
        var label = state switch
        {
            "completed" => "Completed",
            "in-progress" => "In Progress",
            "create-requested" => "In Progress",
            "delete-requested" => "Deleted",
            "deleted" => "Deleted",
            "failed" => "Failed",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        if (state == "completed")
        {
            workspaceToolbox!
                .Find(
                    $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.ExistingId, tool, WorkspaceToolboxPage.ConfigureButtonId])}")
                .Should()
                .NotBeNull();
            workspaceToolbox!
                .Find(
                    $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.ExistingId, tool, WorkspaceToolboxPage.RemoveButtonId])}")
                .Should()
                .NotBeNull();
        }
        else
        {
            var toolStatusLabel =
                workspaceToolbox!.Find(
                    $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.ExistingId, tool, WorkspaceToolboxPage.ToolStatusLabelId])}");
            toolStatusLabel.TextContent.Should().Be(label);
        }
    }

    [When(@"the user clicks on the information sheet for (.*)")]
    public void WhenTheUserClicksOnTheInformationSheetFor(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var infoButton =
            workspaceToolbox!.Find(
                $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CatalogId, tool, ToolboxItem.InfoSheetButtonId])}");
        infoButton.Click();
    }

    [Then(@"the user should see the information sheet for (.*)")]
    public void ThenTheUserShouldSeeTheInformationSheetFor(string tool)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!
            .Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CatalogId, tool, InfoSheet.InfoSheetId])}")
            .Should()
            .NotBeNull();
    }

    [Then(@"the user should see the configuration form for (.*) with (.*)")]
    public void ThenTheUserShouldSeeTheConfigurationFormFor(string tool, string formId)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Find($"#{formId}").Should().NotBeNull();
    }

    [When(@"the user sets (.*) in the form to (.*)")]
    public void WhenTheUserMakesChangesToTheInTheForm(string selectFieldId, string newValue)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var selectField = workspaceToolbox!.Find($"#{selectFieldId}");
        selectField.Click();
        workspaceToolbox!.Render();
        var newValueItem = workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ElementId([selectFieldId, newValue])}");
        newValueItem.Click();
    }

    [Then(@"the (.*) should have (.*) as its value")]
    public void ThenTheShouldHaveAsItsValue(string selectFieldId, string existingValue)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var selectField = workspaceToolbox!.Find($"#{selectFieldId}");
        selectField.Attributes["value"]?.Value.Should().Be(existingValue);
    }

    [Then(@"the underlying Configure transaction should show the correct (.*) and (.*) values for (.*)")]
    public void ThenTheUnderlyingConfigureTransactionShouldShowTheCorrectAndValuesFor(string existingValue,
        string newValue, string fieldName)
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var configurable = (bool)scenarioContext["configurable"];
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        if (configurable)
        {
            var tool = (string)scenarioContext["tool"];
            Testing.GetPrivateField(workspaceToolbox!, "_transactions", out var transactions);
            var transaction = (transactions as List<ToolboxTransaction>)!
                .FirstOrDefault(t => t.Tool == tool && t.Type == ToolboxTransactionType.Update);
            var ogDataStr = JsonSerializer.Serialize(transaction!.OriginalData);
            var ogData = JsonSerializer.Deserialize<JsonObject>(ogDataStr);
            string ogValue = ogData![fieldName]?.ToString() ?? string.Empty;
            ogValue.Should().Be(existingValue);
            var newDataStr = JsonSerializer.Serialize(transaction!.UpdatedData);
            var newData = JsonSerializer.Deserialize<JsonObject>(newDataStr);
            string newValueStr = newData![fieldName]?.ToString() ?? string.Empty;
            newValueStr.Should().Be(newValue);
        }
    }

    [Then(@"the user should see review information for (.*) with the (.*) and (.*)")]
    public void ThenTheUserShouldSeeReviewInformationForWithTheAnd(string tool, string existingValue, string newValue)
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Render();
        var reviewInfo =
            workspaceToolbox!.Find(
                $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.ReviewConfigurationId, tool])}");
        if (existingValue != "null")
        {
            reviewInfo.TextContent.Should().Contain(existingValue);
        }

        reviewInfo.TextContent.Should().Contain(newValue);
    }

    [When(@"the user clicks the Next button again, if it is (.*)")]
    public void WhenTheUserClicksTheNextButtonAgainIfItIs(bool configurable)
    {
        if (configurable)
        {
            var workspaceToolbox =
                scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
            workspaceToolbox!.Render();
            var nextButton =
                workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.NextButtonId])}");
            nextButton.Click();
        }

        scenarioContext["configurable"] = configurable;
    }

    [Then(@"at this stage, the generated workspace definition should be correct, with the correct (.*) value")]
    public void ThenAtThisStageTheGeneratedWorkspaceDefinitionShouldBeCorrect(string configVal)
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        var tool = scenarioContext["tool"] as string;
        var configurable = scenarioContext.Get<bool>("configurable");
        workspaceToolbox!.Render();
        Testing.GetPrivateField(workspaceToolbox!, "_transactions", out var transactions);
        Testing.GetPrivateField(workspaceToolbox!, "_workspaceDefinition", out var workspaceDefinition);
        var toolboxService = Services.GetService<IToolboxService>();
        var generatedDefinition = toolboxService.ApplyTransaction(workspaceDefinition as WorkspaceDefinition,
            transactions as List<ToolboxTransaction>);
        var dependencies = TerraformTemplate.GetDependenciesToCreate(tool) ?? [];

        generatedDefinition.Should().NotBeNull();
        generatedDefinition.Templates.Should().HaveCount(dependencies.Count + 1);
        generatedDefinition.Templates.All(t =>
            dependencies.Select(d => d.Name).Contains(t.Name) ||
            t.Name == tool).Should().BeTrue();
        generatedDefinition.Templates.All(r => r.Status == TerraformStatus.CreateRequested).Should().BeTrue();

        if (configurable)
        {
            switch (tool)
            {
                case TerraformTemplate.AzurePostgres:
                    generatedDefinition.AppData.PostgresConfiguration.Should().NotBeNull();
                    generatedDefinition.AppData.PostgresConfiguration.PSQL_SKU.Should().NotBeNull();
                    generatedDefinition.AppData.PostgresConfiguration.PSQL_SKU.Should().Be(configVal);
                    break;
            }
        }

        scenarioContext["configVal"] = configVal;
    }

    [When(@"the user clicks the Complete button")]
    public void WhenTheUserClicksTheCompleteButton()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Render();
        var completeButton =
            workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CompleteButtonId])}");
        completeButton.Click();
    }

    [Then(@"the user should see the request submission steps")]
    public void ThenTheUserShouldSeeTheRequestSubmissionSteps()
    {
        var workspaceToolbox = scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        workspaceToolbox!.Render();
        var completionSteps =
            workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CompletionStepsId])}");
        completionSteps.Children.Length.Should().Be(3);
    }

    [When(@"the user waits for  (.*) sec")]
    public async Task WhenTheUserWaitsForSec(int seconds)
    {
        await Task.Delay(TimeSpan.FromSeconds(seconds));
    }

    [Then(@"the user should see the completed submission steps")]
    public void ThenTheUserShouldSeeTheCompletedSubmissionSteps()
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        workspaceToolbox!.Render();
        Testing.GetPrivateField(workspaceToolbox!, "_adminEventLogs", out var eventLogsObj);
        var eventLogs = eventLogsObj as List<string>;
        var completionSteps =
            workspaceToolbox!.FindAll(
                $"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CompletionStepsId, WorkspaceToolboxPage.CompletedState])}");
        var completionStepsTitle =
            workspaceToolbox!.Find($"#{WorkspaceToolboxPage.ElementId([WorkspaceToolboxPage.CompletionStepTitleId])}")
                .TextContent;
        completionSteps.Count.Should().Be(3);
        completionStepsTitle.Should().Contain("Request submitted successfully");
    }

    [Then(@"the database should contain the corresponding changes")]
    public async Task ThenTheDatabaseShouldContainTheCorrespondingChanges()
    {
        var configurable = scenarioContext.Get<bool>("configurable");
        var configValue = scenarioContext.Get<string>("configVal");
        var tool = scenarioContext["tool"] as string;
        var dependencies = TerraformTemplate.GetDependenciesToCreate(tool) ?? [];
        using var ctx = await dbContextFactory.CreateDbContextAsync();
        var resources = ctx.Project_Resources2.ToList();
        resources.Should().HaveCount(dependencies.Count + 1);
        resources.All(r =>
            dependencies.Select(d => TerraformTemplate.GetTerraformServiceType(d.Name)).Contains(r.ResourceType) ||
            r.ResourceType == TerraformTemplate.GetTerraformServiceType(tool)).Should().BeTrue();
        resources.All(r => r.Status == TerraformStatus.CreateRequested).Should().BeTrue();

        if (configurable)
        {
            switch (tool)
            {
                case TerraformTemplate.AzurePostgres:
                    var toolResource = resources.FirstOrDefault(r =>
                        r.ResourceType == TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres));
                    toolResource.Should().NotBeNull();
                    toolResource.InputJsonContent.Should().NotBeNull();
                    var inputJsonContent = toolResource.InputJsonContent.ToString();
                    inputJsonContent.Should().Contain(configValue);
                    break;
            }
        }
    }

    [Then(@"the request should have been properly sent to the resource provisioner")]
    public void ThenTheRequestShouldHaveBeenProperlySentToTheResourceProvisioner()
    {
        var workspaceToolboxContainer =
            scenarioContext["workspaceToolbox"] as IRenderedComponent<CascadingAuthenticationState>;
        var workspaceToolbox = workspaceToolboxContainer!.FindComponent<WorkspaceToolboxPage>();
        Testing.GetPrivateField(workspaceToolbox!, "_sentToTerraform", out var sentToTerraform);
        var isSentToTerraform = sentToTerraform is bool ? (bool)sentToTerraform : false;
        isSentToTerraform.Should().BeTrue();
    }

    [Then(@"the user should be redirected to the workspace dashboard")]
    public void ThenTheUserShouldBeRedirectedToTheWorkspaceDashboard()
    {
    }
}

public class TestAuthStateProvider : AuthenticationStateProvider
{
    private readonly TestAuthorizationContext _context;

    public TestAuthStateProvider(TestAuthorizationContext context)
    {
        _context = context;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_context.User));
    }
}

public class TestAuthorizationContext
{
    public ClaimsPrincipal User { get; set; }
}

public class TestAuthorizationService : IAuthorizationService
{
    private readonly TestAuthorizationContext _context;

    public TestAuthorizationService(TestAuthorizationContext context)
    {
        _context = context;
    }

    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource,
        IEnumerable<IAuthorizationRequirement> requirements)
    {
        return Task.FromResult(AuthorizationResult.Success());
    }

    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource,
        string policyName)
    {
        return Task.FromResult(AuthorizationResult.Success());
    }
}

public class TestAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        Task.FromResult(new AuthorizationPolicy(new[] { new DenyAnonymousAuthorizationRequirement() },
            Array.Empty<string>()));

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName) =>
        Task.FromResult<AuthorizationPolicy?>(
            new AuthorizationPolicy(new[] { new DenyAnonymousAuthorizationRequirement() }, Array.Empty<string>()));

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        Task.FromResult<AuthorizationPolicy?>(null);
}