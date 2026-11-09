using System.Text.Json;
using Bunit;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Portal.Components;
using Datahub.Shared.Entities;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;
using static Datahub.Core.Model.Projects.Project_Role;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public sealed class AccessibleEnvironmentVariablesTableSteps : BunitTestSteps, IDisposable
{
    private const string WorkspaceAcronym = "ENVTEST";
    private readonly IKeyVaultUserService _keyVault = Substitute.For<IKeyVaultUserService>();
    private IRenderedComponent<EnvironmentVariablesTable>? _component;

    [Given("environment variables are displayed for an administrator")]
    public void GivenEnvironmentVariablesAreDisplayedForAnAdministrator()
        => RenderTable(RoleNames.Admin, RoleConstants.ADMIN_SUFFIX);

    [Given("environment variables are displayed for a guest")]
    public void GivenEnvironmentVariablesAreDisplayedForAGuest()
        => RenderTable(RoleNames.Guest, RoleConstants.GUEST_SUFFIX);

    [Then("a single GCDS environment variables table is displayed")]
    public void ThenASingleGcdsEnvironmentVariablesTableIsDisplayed()
    {
        var component = GetComponent();
        component.FindComponents<GcdsTable>().Should().ContainSingle();
        component.FindComponents<MudTable<Datahub.Portal.Components.KeyValuePair>>().Should().BeEmpty();

        var table = GetTable();
        table.Filter.Should().BeTrue();
        table.Sort.Should().BeTrue();
        table.Pagination.Should().BeFalse();
        component.Find("[slot='caption']").TextContent.Trim().Should().Be("Environment variables");

        var columns = SerializeToElement(table.Columns).EnumerateArray().ToArray();
        columns.Select(column => column.GetProperty("header").GetString()).Should().Equal("Key", "Value");
        columns[0].GetProperty("rowHeader").GetBoolean().Should().BeTrue();
        columns[1].GetProperty("rowHeader").GetBoolean().Should().BeFalse();
    }

    [Then("the environment variable values are masked")]
    public void ThenTheEnvironmentVariableValuesAreMasked()
    {
        var rows = GetRows();
        rows["API_KEY"].Should().MatchRegex("^\\*+$");
        rows.Values.Should().NotContain("private-value");
    }

    [When("I reveal the environment variable values")]
    public Task WhenIRevealTheEnvironmentVariableValues()
        => ClickButtonAsync("Show environment variable values");

    [Then("available environment variable values are revealed")]
    public void ThenAvailableEnvironmentVariableValuesAreRevealed()
        => GetRows()["API_KEY"].Should().Be("private-value");

    [Then("missing environment variable values use the localized fallback")]
    public void ThenMissingEnvironmentVariableValuesUseTheLocalizedFallback()
        => GetRows()["MISSING_KEY"].Should().Be("Could not find value");

    [Then("environment variable administration controls are not displayed")]
    public void ThenEnvironmentVariableAdministrationControlsAreNotDisplayed()
    {
        var component = GetComponent();
        component.FindAll("#environment-variable-editor").Should().BeEmpty();
        component.FindComponents<GcdsButton>().Should().BeEmpty();
    }

    [When("I try to add an invalid environment variable")]
    public async Task WhenITryToAddAnInvalidEnvironmentVariable()
    {
        await ClickButtonAsync("Add environment variable");
        await SetInputAsync("environment-variable-key", "INVALID-KEY");
        await SetInputAsync("environment-variable-value", string.Empty);
        await ClickButtonAsync("Save");
    }

    [Then("the environment variable validation errors are displayed")]
    public void ThenTheEnvironmentVariableValidationErrorsAreDisplayed()
    {
        FindInput("environment-variable-key").Instance.ErrorMessage
            .Should().Be("Key can only contain letters and underscores");
        FindInput("environment-variable-value").Instance.ErrorMessage.Should().Be("Value cannot be empty");
    }

    [Then("the environment variable is not saved")]
    public void ThenTheEnvironmentVariableIsNotSaved()
        => _keyVault.DidNotReceiveWithAnyArgs().StoreOrUpdateSecret(default!, default!, default!);

    [When("I add a valid environment variable")]
    public async Task WhenIAddAValidEnvironmentVariable()
    {
        await ClickButtonAsync("Add environment variable");
        await SetInputAsync("environment-variable-key", "new_key");
        await SetInputAsync("environment-variable-value", "new-value");
        await ClickButtonAsync("Save");
    }

    [Then("the environment variable table is refreshed")]
    public void ThenTheEnvironmentVariableTableIsRefreshed()
    {
        GetRows()["NEW_KEY"].Should().Be("*********");
        _keyVault.Received(1).StoreOrUpdateSecret(WorkspaceAcronym, "new-key", "new-value");
    }

    [Then("the restart warning is displayed")]
    public void ThenTheRestartWarningIsDisplayed()
    {
        GetComponent().Instance.needsRestart.Should().BeTrue();
        GetComponent().FindComponent<GcdsNotice>().Markup.Should().Contain("Restart required");
    }

    [When("I edit an existing environment variable")]
    public async Task WhenIEditAnExistingEnvironmentVariable()
    {
        var selection = GetComponent().FindComponent<GcdsSelect>();
        await selection.InvokeAsync(() => selection.Instance.ValueChanged.InvokeAsync("API_KEY"));
        FindInput("environment-variable-key").Instance.Disabled.Should().BeTrue();
        await SetInputAsync("environment-variable-value", "updated-value");
        await ClickButtonAsync("Save");
    }

    [Then("the environment variable key is immutable")]
    public void ThenTheEnvironmentVariableKeyIsImmutable()
        => _keyVault.Received(1).StoreOrUpdateSecret(WorkspaceAcronym, "api-key", "updated-value");

    [Then("the environment variable value is saved")]
    public void ThenTheEnvironmentVariableValueIsSaved()
        => GetRows()["API_KEY"].Should().Be("*************");

    private void RenderTable(RoleNames roleName, string authorizationRoleSuffix)
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddMudMarkdownServices();

        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()]
            .Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
        localizer[Arg.Any<string>(), Arg.Any<object[]>()]
            .Returns(call => new LocalizedString(
                call.ArgAt<string>(0),
                string.Format(call.ArgAt<string>(0), call.ArgAt<object[]>(1))));
        Services.AddSingleton(localizer);

        var user = new PortalUser
        {
            Id = 1,
            Email = "environment.variables@example.test"
        };
        var resource = CreateDatabase(user, roleName);

        var userInformation = Substitute.For<IUserInformationService>();
        userInformation.GetCurrentPortalUserAsync().Returns(user);
        Services.AddSingleton(userInformation);
        Services.AddSingleton(_keyVault);
        Services.AddSingleton(Substitute.For<ILogger<EnvironmentVariablesTable>>());
        Services.AddSingleton(Substitute.For<ISnackbar>());

        _keyVault.GetSecretAsync(WorkspaceAcronym, "api-key").Returns("private-value");
        _keyVault.GetSecretAsync(WorkspaceAcronym, "missing-key").Returns((string?)null);

        var authorization = this.AddAuthorization();
        authorization.SetAuthorized("Environment variables user");
        authorization.SetRoles($"{WorkspaceAcronym}{authorizationRoleSuffix}");

        _component = Render<EnvironmentVariablesTable>(parameters => parameters
            .Add(component => component.resource, resource)
            .Add(component => component.projectAcronym, WorkspaceAcronym)
            .Add(component => component.isEditable, true));
    }

    private Project_Resources2 CreateDatabase(PortalUser user, RoleNames roleName)
    {
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var factory = new SpecFlowDbContextFactory(options);
        Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(factory);

        var role = Project_Role.GetAll().Single(candidate => candidate.Id == (int)roleName);
        var project = new Datahub_Project
        {
            Project_ID = 1,
            Project_Acronym_CD = WorkspaceAcronym,
            Project_Name = "Environment variables workspace",
            Project_Status_Desc = "InProgress",
            Project_Icon = "database",
            Project_Summary_Desc = "Environment variables test workspace"
        };
        var resource = new Project_Resources2
        {
            ResourceId = Guid.NewGuid(),
            ProjectId = project.Project_ID,
            Project = project,
            ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureAppService),
            InputJsonContent = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["environment_variables_keys"] = JsonSerializer.Serialize(new[] { "API_KEY", "MISSING_KEY" })
            })
        };

        using var context = factory.CreateDbContext();
        context.UserRolesLinks.Add(new UserRoleLinks
        {
            PortalUser = user,
            PortalUserId = user.Id,
            Project = project,
            Project_ID = project.Project_ID,
            Role = role,
            RoleId = role.Id
        });
        context.Project_Resources2.Add(resource);
        context.SaveChanges();
        return resource;
    }

    private async Task SetInputAsync(string id, string value)
    {
        var input = FindInput(id);
        await input.InvokeAsync(() => input.Instance.ValueChanged.InvokeAsync(value));
    }

    private async Task ClickButtonAsync(string text)
    {
        var button = GetComponent().FindComponents<GcdsButton>()
            .Single(candidate => candidate.Markup.Contains(text, StringComparison.Ordinal));
        await button.InvokeAsync(() => button.Instance.OnClick.InvokeAsync());
    }

    private Dictionary<string, string> GetRows()
        => SerializeToElement(GetTable().Data).EnumerateArray().ToDictionary(
            row => row.GetProperty("key").GetString()!,
            row => row.GetProperty("value").GetString()!);

    private IRenderedComponent<GcdsInput> FindInput(string id)
        => GetComponent().FindComponents<GcdsInput>().Single(input => input.Instance.Id == id);

    private GcdsTable GetTable() => GetComponent().FindComponent<GcdsTable>().Instance;

    private IRenderedComponent<EnvironmentVariablesTable> GetComponent()
        => _component ?? throw new InvalidOperationException("The environment variables table has not been rendered.");

    private static JsonElement SerializeToElement(object? value)
        => JsonSerializer.SerializeToElement(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    void IDisposable.Dispose()
    {
        // BunitTestSteps disposes the context asynchronously in its AfterScenario hook.
    }
}
