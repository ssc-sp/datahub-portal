using System.Net;
using System.Reflection;
using System.Text.Json;
using Bunit;
using Datahub.Application.Configuration;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Portal.Pages.Workspace.Database;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class IpAddressWhitelistSteps(ScenarioContext scenarioContext) : BunitTestSteps
{
    private const string RuleName = "office-rule";
    private IRenderedComponent<DatabaseIpWhitelistTable>? _whitelist;

    [Given(@"a workspace and an azure subscription id for an DatabaseIpWhitelistTable component")]
    public async Task GivenAWorkspaceAndAnAzureSubscriptionIdForAnDatabaseIpWhitelistTableComponent()
    {
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        var dbContextFactory = new SpecFlowDbContextFactory(options);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        
        // First, add and save the subscription
        var azureSubscription = new DatahubAzureSubscription
        {
            SubscriptionId = Testing.WorkspaceSubscriptionGuid,
            SubscriptionName = Testing.SubscriptionName,
            TenantId = Testing.WorkspaceTenantGuid,
        };
        dbContext.AzureSubscriptions.Add(azureSubscription);
        await dbContext.SaveChangesAsync();

        // Then create and add the workspace with the subscription ID reference
        var workspace = new Datahub_Project
        {
            Project_Name = Testing.WorkspaceName,
            Project_Acronym_CD = Testing.WorkspaceAcronym,
            DatahubAzureSubscription = azureSubscription,
            DatahubAzureSubscriptionId = azureSubscription.Id
        };
        dbContext.Projects.Add(workspace);
        await dbContext.SaveChangesAsync();
        
        scenarioContext["dbContextFactory"] = dbContextFactory;
    }

    [When(@"the workspace subscription id is retrieved")]
    public async Task WhenTheWorkspaceSubscriptionIdIsRetrieved()
    {
        var dbContextFactory = scenarioContext["dbContextFactory"] as IDbContextFactory<DatahubProjectDBContext>;
        await using var dbContext = await dbContextFactory!.CreateDbContextAsync();
        
        var workspaceSubscriptionId = await DatabaseIpWhitelistTable.RetrieveWorkspaceSubscriptionId(Testing.WorkspaceAcronym, dbContext);
        scenarioContext["WorkspaceSubscriptionId"] = workspaceSubscriptionId;
    }

    [Then(@"the workspace subscription id should be the same as the azure subscription id")]
    public void ThenTheWorkspaceSubscriptionIdShouldBeTheSameAsTheAzureSubscriptionId()
    {
        var workspaceSubscriptionId = scenarioContext["WorkspaceSubscriptionId"] as string;
        workspaceSubscriptionId.Should().Be(Testing.WorkspaceSubscriptionGuid);
    }

    [Given("a rendered DatabaseIpWhitelistTable component")]
    public void GivenARenderedDatabaseIpWhitelistTableComponent()
    {
        JSInterop.SetupMudBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();

        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()]
            .Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
        localizer[Arg.Any<string>(), Arg.Any<object[]>()]
            .Returns(call => new LocalizedString(
                call.ArgAt<string>(0),
                string.Format(call.ArgAt<string>(0), call.ArgAt<object[]>(1))));

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.10");

        Services.AddSingleton(localizer);
        Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = httpContext });
        Services.AddSingleton(Substitute.For<IDbContextFactory<DatahubProjectDBContext>>());
        Services.AddSingleton(new DatahubPortalConfiguration());
        Services.AddSingleton(Substitute.For<ISnackbar>());
        Services.AddSingleton(Substitute.For<IUserInformationService>());

        _whitelist = Render<DatabaseIpWhitelistTable>(parameters => parameters
            .Add(component => component.WorkspaceAcronym, Testing.WorkspaceAcronym));
    }

    [When("a sample firewall rule is displayed")]
    public void WhenASampleFirewallRuleIsDisplayed()
    {
        var rulesField = typeof(DatabaseIpWhitelistTable).GetField(
            "_firewallRules",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var rules = rulesField?.GetValue(GetWhitelist().Instance) as List<WhitelistIPAddressData>;

        rules.Should().NotBeNull();
        rules!.Add(new WhitelistIPAddressData
        {
            Name = RuleName,
            StartIPAddress = IPAddress.Parse("192.0.2.1"),
            EndIPAddress = IPAddress.Parse("192.0.2.20")
        });
        GetWhitelist().Render();
    }

    [Then("the firewall rules use a GCDS table")]
    public void ThenTheFirewallRulesUseAGcdsTable()
    {
        GetWhitelist().FindComponents<GcdsTable>().Should().ContainSingle();
        GetWhitelist().FindComponents<MudTable<WhitelistIPAddressData>>().Should().BeEmpty();

        var caption = GetWhitelist().Find("[slot='caption']");
        caption.TextContent.Trim().Should().Be("Database IP Address Whitelist");
        caption.ClassList.Should().Contain("sr-only");
    }

    [Then("the firewall table has localized accessible columns")]
    public void ThenTheFirewallTableHasLocalizedAccessibleColumns()
    {
        var columns = SerializeToElement(GetTable().Columns).EnumerateArray().ToArray();
        columns.Should().HaveCount(3);
        columns.Select(column => column.GetProperty("header").GetString())
            .Should().Equal("Name", "Start IP Address", "End IP Address");
        columns[0].GetProperty("rowHeader").GetBoolean().Should().BeTrue();
        columns.Skip(1).Should().OnlyContain(column => !column.GetProperty("rowHeader").GetBoolean());
    }

    [Then("the firewall table supports filtering and sorting")]
    public void ThenTheFirewallTableSupportsFilteringAndSorting()
    {
        var table = GetTable();
        table.Filter.Should().BeTrue();
        table.Sort.Should().BeTrue();
        table.Pagination.Should().BeFalse();
    }

    [Then("the firewall rule values are displayed as plain text")]
    public void ThenTheFirewallRuleValuesAreDisplayedAsPlainText()
    {
        var row = SerializeToElement(GetTable().Data).EnumerateArray().Single();
        row.GetProperty("name").GetString().Should().Be(RuleName);
        row.GetProperty("startIpAddress").GetString().Should().Be("192.0.2.1");
        row.GetProperty("endIpAddress").GetString().Should().Be("192.0.2.20");
    }

    [Then("the firewall rule actions are disabled")]
    public void ThenTheFirewallRuleActionsAreDisabled()
    {
        GetActionButton("Edit").Instance.Disabled.Should().BeTrue();
        GetActionButton("Delete").Instance.Disabled.Should().BeTrue();
    }

    [When("the sample firewall rule is selected")]
    public async Task WhenTheSampleFirewallRuleIsSelected()
    {
        var select = GetWhitelist().FindComponent<GcdsSelect>();
        await GetWhitelist().InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(RuleName));
    }

    [Then("the firewall rule actions are enabled")]
    public void ThenTheFirewallRuleActionsAreEnabled()
    {
        GetActionButton("Edit").Instance.Disabled.Should().BeFalse();
        GetActionButton("Delete").Instance.Disabled.Should().BeFalse();
    }

    [When("the selected firewall rule is edited")]
    public async Task WhenTheSelectedFirewallRuleIsEdited()
    {
        await GetWhitelist().InvokeAsync(GetActionButton("Edit").Instance.OnClick.InvokeAsync);
    }

    [Then("the firewall rule editor is an inline GCDS form")]
    public void ThenTheFirewallRuleEditorIsAnInlineGcdsForm()
    {
        var inputs = GetWhitelist().FindComponents<GcdsInput>();
        inputs.Select(input => input.Instance.Id).Should().Equal(
            "firewall-rule-name",
            "firewall-rule-start-ip",
            "firewall-rule-end-ip");
        inputs.Select(input => input.Instance.Value).Should().Equal(
            RuleName,
            "192.0.2.1",
            "192.0.2.20");
        inputs.Should().AllSatisfy(input => input.Instance.ValueExpression.Should().NotBeNull());

        GetWhitelist().Find("[role='region'][aria-labelledby='firewall-rule-form-heading']");
        GetActionButton("Save").Should().NotBeNull();
        GetActionButton("Cancel").Should().NotBeNull();
        GetWhitelist().FindComponents<MudDialog>().Should().BeEmpty();
        AssertRuleFormIsBelow("Edit");
    }

    [When("the add firewall rule form is opened")]
    public async Task WhenTheAddFirewallRuleFormIsOpened()
    {
        await GetWhitelist().InvokeAsync(() =>
            GetActionButton("Add a new IP address").Instance.OnClick.InvokeAsync(new MouseEventArgs()));
    }

    [Then("the add firewall rule editor is an inline GCDS form")]
    public void ThenTheAddFirewallRuleEditorIsAnInlineGcdsForm()
    {
        var inputs = GetWhitelist().FindComponents<GcdsInput>();
        inputs.Select(input => input.Instance.Id).Should().Equal(
            "firewall-rule-start-ip",
            "firewall-rule-end-ip");
        inputs.Should().AllSatisfy(input => input.Instance.ValueExpression.Should().NotBeNull());

        GetWhitelist().Find("[role='region'][aria-labelledby='firewall-rule-form-heading']");
        GetActionButton("Save").Should().NotBeNull();
        GetActionButton("Cancel").Should().NotBeNull();
        GetWhitelist().FindComponents<MudDialog>().Should().BeEmpty();
        AssertRuleFormIsBelow("Add a new IP address");
    }

    private void AssertRuleFormIsBelow(string buttonText)
    {
        var markup = GetWhitelist().Markup;
        var buttonIndex = markup.IndexOf(buttonText, StringComparison.Ordinal);
        var formIndex = markup.IndexOf("aria-labelledby=\"firewall-rule-form-heading\"", StringComparison.Ordinal);

        buttonIndex.Should().BeGreaterThanOrEqualTo(0);
        formIndex.Should().BeGreaterThan(buttonIndex);
    }

    private IRenderedComponent<GcdsButton> GetActionButton(string text) => GetWhitelist()
        .FindComponents<GcdsButton>()
        .Single(button => button.Find("gcds-button").TextContent.Trim() == text);

    private static JsonElement SerializeToElement(object? value) =>
        JsonSerializer.SerializeToElement(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private GcdsTable GetTable() => GetWhitelist().FindComponent<GcdsTable>().Instance;

    private IRenderedComponent<DatabaseIpWhitelistTable> GetWhitelist() =>
        _whitelist ?? throw new InvalidOperationException("The database IP whitelist has not been rendered.");
}
