using System.Globalization;
using Bunit;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Projects;
using Datahub.Metadata.Model;
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
public class WorkspaceObjectMetadataSteps(ScenarioContext scenarioContext) : BunitTestSteps
{
    private const string ProjectAcronym = "METADATA";
    private const string ComponentKey = "workspaceObjectMetadata";
    private const string AcronymKey = "workspaceObjectMetadataAcronym";

    [Given("a workspace has complete GC Hosting metadata")]
    public async Task GivenAWorkspaceHasCompleteGcHostingMetadata()
    {
        ConfigureServices();
        await SeedWorkspaceAsync(CreateWorkspaceDetails());
        scenarioContext[AcronymKey] = ProjectAcronym;
    }

    [Given("a workspace has GC Hosting metadata without optional project details")]
    public async Task GivenAWorkspaceHasGcHostingMetadataWithoutOptionalProjectDetails()
    {
        ConfigureServices();
        var details = CreateWorkspaceDetails();
        details.ProjectTitle = null;
        details.ProjectDescription = null;
        await SeedWorkspaceAsync(details);
        scenarioContext[AcronymKey] = ProjectAcronym;
    }

    [Given("a workspace has no GC Hosting metadata")]
    public async Task GivenAWorkspaceHasNoGcHostingMetadata()
    {
        ConfigureServices();
        await SeedWorkspaceAsync();
        scenarioContext[AcronymKey] = ProjectAcronym;
    }

    [Given("no workspace acronym is supplied for GC Hosting metadata")]
    public void GivenNoWorkspaceAcronymIsSuppliedForGcHostingMetadata()
    {
        ConfigureServices();
        scenarioContext[AcronymKey] = string.Empty;
    }

    [When("the workspace GC Hosting metadata is rendered")]
    public void WhenTheWorkspaceGcHostingMetadataIsRendered()
    {
        var acronym = scenarioContext[AcronymKey] as string;
        var component = Render<WorkspaceObjectMetadata>(parameters => parameters
            .Add(metadata => metadata.ProjectAcronym, acronym));

        component.WaitForState(() => component.FindComponents<GcdsText>()
            .All(text => !text.Markup.Contains("Loading...", StringComparison.Ordinal)));
        scenarioContext[ComponentKey] = component;
    }

    [Then("every GC Hosting business metadata value is displayed")]
    public void ThenEveryGcHostingBusinessMetadataValueIsDisplayed()
    {
        var component = GetComponent();
        var inputs = component.FindComponents<GcdsInput>().ToDictionary(input => input.Instance.Id);
        var textareas = component.FindComponents<GcdsTextarea>().ToDictionary(textarea => textarea.Instance.Id);

        var expectedInputs = new Dictionary<string, string>
        {
            ["gc-hosting-id"] = "HOST-123",
            ["department-name"] = "Natural Resources Canada",
            ["workspace-name"] = "Workspace metadata test",
            ["workspace-subject"] = "Earth sciences",
            ["workspace-keywords"] = "earth, science, data",
            ["project-title"] = "Metadata modernization",
            ["lead-name"] = "Ada Lovelace",
            ["lead-email"] = "ada@example.gc.ca",
            ["financial-authority-name"] = "Grace Hopper",
            ["financial-authority-email"] = "grace@example.gc.ca",
            ["commitment-is-ref"] = "REF-456",
            ["commitment-is-org"] = "ORG-789",
            ["cbr-id"] = "CBR-012",
            ["cbr-name"] = "Science data CBR",
            ["workspace-budget"] = 12345.67M.ToString(CultureInfo.CurrentCulture),
            ["retention-period"] = 7.ToString(CultureInfo.CurrentCulture),
            ["retention-start-date"] = new DateTime(2026, 4, 5).ToString("d", CultureInfo.CurrentCulture),
            ["retention-value"] = "Archival value",
            ["generates-information-business-value"] = "Yes",
            ["security-classification"] = "Protected B"
        };

        inputs.Should().HaveCount(expectedInputs.Count);
        foreach (var (id, value) in expectedInputs)
        {
            inputs[id].Instance.Value.Should().Be(value);
        }

        textareas.Should().HaveCount(2);
        textareas["workspace-description"].Instance.Value.Should().Be("Detailed workspace description");
        textareas["project-description"].Instance.Value.Should().Be("Detailed project description");
    }

    [Then("every GC Hosting metadata control is a read-only GCDS control")]
    public void ThenEveryGcHostingMetadataControlIsAReadOnlyGcdsControl()
    {
        var component = GetComponent();

        component.FindComponents<GcdsInput>().Should().OnlyContain(input => input.Instance.ReadOnly);
        component.FindAll("gcds-textarea").Should().OnlyContain(textarea => textarea.HasAttribute("readonly"));
        component.FindComponents<GcdsHeading>().Should().HaveCount(8);
        component.Markup.Should().NotContain("mud-input");
    }

    [Then("the optional project metadata values are empty and read-only")]
    public void ThenTheOptionalProjectMetadataValuesAreEmptyAndReadOnly()
    {
        var component = GetComponent();
        var projectTitle = component.FindComponents<GcdsInput>()
            .Single(input => input.Instance.Id == "project-title").Instance;
        var projectDescription = component.FindComponents<GcdsTextarea>()
            .Single(textarea => textarea.Instance.Id == "project-description");

        projectTitle.Value.Should().BeEmpty();
        projectTitle.ReadOnly.Should().BeTrue();
        projectDescription.Instance.Value.Should().BeEmpty();
        projectDescription.Find("gcds-textarea").HasAttribute("readonly").Should().BeTrue();
    }

    [Then("the GC Hosting metadata empty state is displayed")]
    public void ThenTheGcHostingMetadataEmptyStateIsDisplayed()
    {
        var component = GetComponent();

        component.Markup.Should().Contain("WorkspaceNoMetadata");
        component.FindComponents<GcdsInput>().Should().BeEmpty();
        component.FindComponents<GcdsTextarea>().Should().BeEmpty();
    }

    private void ConfigureServices()
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
    }

    private async Task SeedWorkspaceAsync(GCHostingWorkspaceDetails? details = null)
    {
        var factory = Services.GetRequiredService<IDbContextFactory<DatahubProjectDBContext>>();
        await using var context = await factory.CreateDbContextAsync();
        context.Projects.Add(new Datahub_Project
        {
            Project_Acronym_CD = ProjectAcronym,
            ParentGCHostingBudget = details
        });
        await context.SaveChangesAsync();
    }

    private static GCHostingWorkspaceDetails CreateWorkspaceDetails() => new()
    {
        GcHostingId = "HOST-123",
        LeadFirstName = "Ada",
        LeadLastName = "Lovelace",
        DepartmentName = "Natural Resources Canada",
        LeadEmail = "ada@example.gc.ca",
        FinancialAuthorityFirstName = "Grace",
        FinancialAuthorityLastName = "Hopper",
        FinancialAuthorityCommitmentIsRef = "REF-456",
        FinancialAuthorityCommitmentIsOrg = "ORG-789",
        FinancialAuthorityEmail = "grace@example.gc.ca",
        WorkspaceBudget = 12345.67M,
        WorkspaceName = "Workspace metadata test",
        WorkspaceDescription = "Detailed workspace description",
        Subject = "Earth sciences",
        Keywords = "earth, science, data",
        RetentionPeriodYears = 7,
        RetentionPeriodStartDate = new DateTime(2026, 4, 5),
        RetentionValue = "Archival value",
        GeneratesInfoBusinessValue = true,
        SecurityClassification = ClassificationType.ProtectedB,
        ProjectTitle = "Metadata modernization",
        ProjectDescription = "Detailed project description",
        CBRName = "Science data CBR",
        CBRID = "CBR-012"
    };

    private IRenderedComponent<WorkspaceObjectMetadata> GetComponent() =>
        scenarioContext[ComponentKey].Should().BeAssignableTo<IRenderedComponent<WorkspaceObjectMetadata>>().Subject;
}
