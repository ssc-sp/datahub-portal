using System.Globalization;
using Bunit;
using Datahub.Core.Model.Catalog;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Portal.Components.Projects;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class WorkspaceProjectInformationSteps(ScenarioContext scenarioContext) : BunitTestSteps
{
    private const string ProjectAcronym = "PROJECT";
    private const string ComponentKey = "workspaceProjectInformation";
    private const string AcronymKey = "workspaceProjectInformationAcronym";
    private const string CatalogKey = "workspaceProjectInformationCatalog";

    [Given("a workspace has project information")]
    public async Task GivenAWorkspaceHasProjectInformation()
    {
        ConfigureServices();
        await SeedProjectAsync();
        scenarioContext[AcronymKey] = ProjectAcronym;
    }

    [Given("a workspace has project information without a budget")]
    public async Task GivenAWorkspaceHasProjectInformationWithoutABudget()
    {
        ConfigureServices();
        await SeedProjectAsync(budget: null);
        scenarioContext[AcronymKey] = ProjectAcronym;
    }

    [Given("no matching workspace project exists")]
    public void GivenNoMatchingWorkspaceProjectExists()
    {
        ConfigureServices();
        scenarioContext[AcronymKey] = ProjectAcronym;
    }

    [Given("no workspace project acronym is supplied")]
    public void GivenNoWorkspaceProjectAcronymIsSupplied()
    {
        ConfigureServices();
        scenarioContext[AcronymKey] = string.Empty;
    }

    [Given("a workspace has project information and catalog synchronization will fail")]
    public async Task GivenAWorkspaceHasProjectInformationAndCatalogSynchronizationWillFail()
    {
        var catalog = ConfigureServices();
        catalog.AddCatalogObject(Arg.Any<CatalogObject>())
            .Returns(Task.FromException(new InvalidOperationException("Catalog unavailable")));
        await SeedProjectAsync();
        scenarioContext[AcronymKey] = ProjectAcronym;
    }

    [When("the workspace project information is rendered")]
    public void WhenTheWorkspaceProjectInformationIsRendered()
    {
        var acronym = scenarioContext[AcronymKey] as string;
        var component = Render<WorkspaceProjectInformation>(parameters => parameters
            .Add(information => information.ProjectAcronym, acronym));

        component.WaitForState(() => !component.Markup.Contains("Loading...", StringComparison.Ordinal));
        scenarioContext[ComponentKey] = component;
    }

    [When("both workspace project descriptions are changed")]
    public async Task WhenBothWorkspaceProjectDescriptionsAreChanged()
    {
        var textareas = GetTextareas();
        await GetComponent().InvokeAsync(async () =>
        {
            await textareas["project-description-english"].Instance.ValueChanged.InvokeAsync("Updated English description");
            await textareas["project-description-french"].Instance.ValueChanged.InvokeAsync("Description française mise à jour");
        });
    }

    [When("the English workspace project description is cleared")]
    public async Task WhenTheEnglishWorkspaceProjectDescriptionIsCleared()
    {
        var textarea = GetTextareas()["project-description-english"];
        await GetComponent().InvokeAsync(() => textarea.Instance.ValueChanged.InvokeAsync("   "));
    }

    [When("the workspace project information is saved")]
    public async Task WhenTheWorkspaceProjectInformationIsSaved()
    {
        var saveButton = GetComponent().FindComponents<GcdsButton>()
            .Single(button => button.Instance.Id == "save-project-information");
        await GetComponent().InvokeAsync(saveButton.Instance.OnClick.InvokeAsync);
    }

    [Then("every workspace project information value is displayed")]
    public void ThenEveryWorkspaceProjectInformationValueIsDisplayed()
    {
        var inputs = GetComponent().FindComponents<GcdsInput>().ToDictionary(input => input.Instance.Id);
        var textareas = GetTextareas();

        inputs["project-acronym"].Instance.Value.Should().Be(ProjectAcronym);
        inputs["project-title-english"].Instance.Value.Should().Be("English project title");
        inputs["project-title-french"].Instance.Value.Should().Be("Titre français du projet");
        inputs["project-budget"].Instance.Value.Should().Be(12345.67M.ToString(CultureInfo.CurrentCulture));
        textareas["project-description-english"].Instance.Value.Should().Be("English project description");
        textareas["project-description-french"].Instance.Value.Should().Be("Description française du projet");
    }

    [Then("only the workspace project descriptions are editable")]
    public void ThenOnlyTheWorkspaceProjectDescriptionsAreEditable()
    {
        GetComponent().FindComponents<GcdsInput>().Should().OnlyContain(input => input.Instance.Disabled);
        GetTextareas().Values.Should().OnlyContain(textarea => textarea.Instance.Required);
        GetTextareas().Values.Should().OnlyContain(textarea => !textarea.Instance.Disabled);
    }

    [Then("the project information save action is initially disabled")]
    public void ThenTheProjectInformationSaveActionIsInitiallyDisabled()
    {
        GetComponent().FindComponents<GcdsButton>()
            .Single(button => button.Instance.Id == "save-project-information")
            .Instance.Disabled.Should().BeTrue();
    }

    [Then("both workspace project descriptions are persisted")]
    public async Task ThenBothWorkspaceProjectDescriptionsArePersisted()
    {
        await using var context = await GetContextFactory().CreateDbContextAsync();
        var project = await context.Projects.SingleAsync(candidate => candidate.Project_Acronym_CD == ProjectAcronym);

        project.Project_Summary_Desc.Should().Be("Updated English description");
        project.Project_Summary_Desc_Fr.Should().Be("Description française mise à jour");
    }

    [Then("the workspace catalog entry contains the updated descriptions")]
    public async Task ThenTheWorkspaceCatalogEntryContainsTheUpdatedDescriptions()
    {
        var catalog = (IDatahubCatalogSearch)scenarioContext[CatalogKey];
        await catalog.Received(1).AddCatalogObject(Arg.Is<CatalogObject>(entry =>
            entry.ObjectType == CatalogObjectType.Workspace
            && entry.ObjectId == ProjectAcronym
            && entry.Name_English == "English project title"
            && entry.Name_French == "Titre français du projet"
            && entry.Desc_English == "Updated English description"
            && entry.Desc_French == "Description française mise à jour"));
    }

    [Then("the project information success state is displayed")]
    public void ThenTheProjectInformationSuccessStateIsDisplayed()
    {
        GetComponent().Markup.Should().Contain("Workspace project information saved successfully.");
        GetComponent().FindComponent<GcdsNotice>().Instance.NoticeRole.Should().Be("success");
    }

    [Then("the required project description error is displayed")]
    public void ThenTheRequiredProjectDescriptionErrorIsDisplayed()
    {
        GetTextareas()["project-description-english"].Instance.ErrorMessage
            .Should().Be("Project description English is required.");
    }

    [Then("the workspace project information is not persisted or synchronized")]
    public async Task ThenTheWorkspaceProjectInformationIsNotPersistedOrSynchronized()
    {
        await using var context = await GetContextFactory().CreateDbContextAsync();
        var project = await context.Projects.SingleAsync(candidate => candidate.Project_Acronym_CD == ProjectAcronym);
        project.Project_Summary_Desc.Should().Be("English project description");

        var catalog = (IDatahubCatalogSearch)scenarioContext[CatalogKey];
        await catalog.DidNotReceive().AddCatalogObject(Arg.Any<CatalogObject>());
    }

    [Then("the project budget value is empty and disabled")]
    public void ThenTheProjectBudgetValueIsEmptyAndDisabled()
    {
        var budget = GetComponent().FindComponents<GcdsInput>()
            .Single(input => input.Instance.Id == "project-budget").Instance;
        budget.Value.Should().BeEmpty();
        budget.Disabled.Should().BeTrue();
    }

    [Then("the workspace project information empty state is displayed")]
    public void ThenTheWorkspaceProjectInformationEmptyStateIsDisplayed()
    {
        GetComponent().Markup.Should().Contain("WorkspaceProjectNoMetadata");
        GetComponent().FindComponents<GcdsInput>().Should().BeEmpty();
        GetComponent().FindComponents<GcdsTextarea>().Should().BeEmpty();
    }

    [Then("the project information failure state is displayed")]
    public void ThenTheProjectInformationFailureStateIsDisplayed()
    {
        GetComponent().Markup.Should().Contain("Unable to save workspace project information. Please try again.");
        GetComponent().FindComponent<GcdsNotice>().Instance.NoticeRole.Should().Be("danger");
    }

    private IDatahubCatalogSearch ConfigureServices()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddLogging();

        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()]
            .Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
        Services.AddSingleton(localizer);

        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(new SpecFlowDbContextFactory(options));

        var catalog = Substitute.For<IDatahubCatalogSearch>();
        Services.AddSingleton(catalog);
        scenarioContext[CatalogKey] = catalog;
        return catalog;
    }

    private async Task SeedProjectAsync(decimal? budget = 12345.67M)
    {
        await using var context = await GetContextFactory().CreateDbContextAsync();
        context.Projects.Add(new Datahub_Project
        {
            Project_Acronym_CD = ProjectAcronym,
            Project_Name = "English project title",
            Project_Name_Fr = "Titre français du projet",
            Project_Summary_Desc = "English project description",
            Project_Summary_Desc_Fr = "Description française du projet",
            Project_Budget = budget
        });
        await context.SaveChangesAsync();
    }

    private IDbContextFactory<DatahubProjectDBContext> GetContextFactory() =>
        Services.GetRequiredService<IDbContextFactory<DatahubProjectDBContext>>();

    private Dictionary<string, IRenderedComponent<GcdsTextarea>> GetTextareas() =>
        GetComponent().FindComponents<GcdsTextarea>().ToDictionary(textarea => textarea.Instance.Id);

    private IRenderedComponent<WorkspaceProjectInformation> GetComponent() =>
        scenarioContext[ComponentKey]
            .Should().BeAssignableTo<IRenderedComponent<WorkspaceProjectInformation>>().Subject;
}
