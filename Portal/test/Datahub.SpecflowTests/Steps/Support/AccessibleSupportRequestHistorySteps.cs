using System.Text.Json;
using Bunit;
using Datahub.Portal.Pages.Help;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MudBlazor;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Support;

[Binding]
public sealed class AccessibleSupportRequestHistorySteps : BunitTestSteps, IDisposable
{
    private const string LocalizedStatus = "Localized active status";
    private IRenderedComponent<SupportRequestHistoryTable>? _history;

    [Given("the support request history is loading")]
    public void GivenTheSupportRequestHistoryIsLoading()
    {
        ConfigureServices();
        _history = Render<SupportRequestHistoryTable>(parameters => parameters
            .Add(component => component.Issues, Array.Empty<IssueForDisplaying>())
            .Add(component => component.Loading, true));
    }

    [Given("the support request history is loaded")]
    public void GivenTheSupportRequestHistoryIsLoaded()
    {
        ConfigureServices();
        _history = Render<SupportRequestHistoryTable>(parameters => parameters
            .Add(component => component.Issues, [CreateIssue()])
            .Add(component => component.Loading, false));
    }

    [Then("the support request history loading state is announced")]
    public void ThenTheSupportRequestHistoryLoadingStateIsAnnounced()
    {
        var status = GetHistory().Find("[role='status']");
        status.GetAttribute("aria-live").Should().Be("polite");
        status.TextContent.Trim().Should().Be("Loading...");
    }

    [Then("the support request history table is hidden")]
    public void ThenTheSupportRequestHistoryTableIsHidden()
    {
        GetHistory().FindComponents<GcdsTable>().Should().BeEmpty();
    }

    [Then("the support request history uses a GCDS table")]
    public void ThenTheSupportRequestHistoryUsesAGcdsTable()
    {
        var history = GetHistory();
        history.FindComponents<GcdsTable>().Should().ContainSingle();
        history.FindComponents<MudTable<IssueForDisplaying>>().Should().BeEmpty();
        history.FindComponents<MudChip<string>>().Should().BeEmpty();
    }

    [Then("the support request history has an accessible caption")]
    public void ThenTheSupportRequestHistoryHasAnAccessibleCaption()
    {
        var caption = GetHistory().Find("[slot='caption']");
        caption.TextContent.Trim().Should().Be("Your Support Requests");
        caption.ClassList.Should().Contain("sr-only");
    }

    [Then("the support request history has six localized columns")]
    public void ThenTheSupportRequestHistoryHasSixLocalizedColumns()
    {
        var columns = SerializeToElement(GetTable().Columns);
        columns.GetArrayLength().Should().Be(6);
        columns.EnumerateArray()
            .Select(column => column.GetProperty("header").GetString())
            .Should().Equal("ID", "Title", "Description", "Status", "Submitted_DT", "Last Update");
    }

    [Then("the support request ID is the row header")]
    public void ThenTheSupportRequestIdIsTheRowHeader()
    {
        var columns = SerializeToElement(GetTable().Columns).EnumerateArray().ToArray();
        columns[0].GetProperty("field").GetString().Should().Be("id");
        columns[0].GetProperty("rowHeader").GetBoolean().Should().BeTrue();
        columns.Skip(1).Should().OnlyContain(column => !column.GetProperty("rowHeader").GetBoolean());
    }

    [Then("the support request fields use localized plain text")]
    public void ThenTheSupportRequestFieldsUseLocalizedPlainText()
    {
        var row = SerializeToElement(GetTable().Data).EnumerateArray().Single();
        row.GetProperty("id").GetString().Should().Be("42");
        row.GetProperty("title").GetString().Should().Be("Cannot access storage");
        row.GetProperty("description").GetString().Should().Be("Storage access is failing.");
        row.GetProperty("status").GetString().Should().Be(LocalizedStatus);
        row.GetProperty("submittedDate").GetString().Should().Be("2026-09-12");
        row.GetProperty("lastUpdate").GetString().Should().Be("2026-09-13");
    }

    [Then("the support request history is paginated by ten rows")]
    public void ThenTheSupportRequestHistoryIsPaginatedByTenRows()
    {
        var table = GetTable();
        table.Pagination.Should().BeTrue();
        table.PaginationSize.Should().Be(10);
        table.Filter.Should().BeFalse();
        table.Sort.Should().BeFalse();
    }

    private void ConfigureServices()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()]
            .Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
        localizer["Your request is being worked on."]
            .Returns(new LocalizedString("Your request is being worked on.", LocalizedStatus));
        Services.AddSingleton(localizer);
    }

    private static IssueForDisplaying CreateIssue()
    {
        var workItem = new WorkItem
        {
            Id = 42,
            Fields = new Dictionary<string, object>
            {
                ["System.Title"] = "Cannot access storage",
                ["System.Description"] = "<strong>Description:</strong> Storage access is failing.<br>",
                ["System.State"] = "Active",
                ["System.CreatedDate"] = "2026-09-12",
                ["System.ChangedDate"] = "2026-09-13"
            }
        };

        return new IssueForDisplaying(workItem);
    }

    private static JsonElement SerializeToElement(object? value)
    {
        return JsonSerializer.SerializeToElement(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private GcdsTable GetTable()
    {
        return GetHistory().FindComponent<GcdsTable>().Instance;
    }

    private IRenderedComponent<SupportRequestHistoryTable> GetHistory()
    {
        return _history ?? throw new InvalidOperationException("The support request history has not been rendered.");
    }

    void IDisposable.Dispose()
    {
        // BunitTestSteps disposes the context asynchronously in its AfterScenario hook.
    }
}
