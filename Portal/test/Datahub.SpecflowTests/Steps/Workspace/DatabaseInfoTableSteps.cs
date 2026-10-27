using System.Text.Json;
using Bunit;
using Datahub.Portal.Model;
using Datahub.Portal.Pages.Workspace.Database;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class DatabaseInfoTableSteps : BunitTestSteps
{
    private readonly AzurePgsqlDBServer _databaseServer = new()
    {
        DatabaseHost = "database.example",
        DatabaseName = "workspace",
        Username = "database-user",
        Password = "database-password",
        Port = "5432"
    };

    private IRenderedComponent<DatabaseInfoTable>? _databaseInfoTable;

    [Given("a database information table with visible connection information")]
    public void GivenADatabaseInformationTableWithVisibleConnectionInformation()
    {
        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()]
            .Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
        Services.AddSingleton(localizer);

        _databaseInfoTable = Render<DatabaseInfoTable>(parameters => parameters
            .Add(component => component.dbServer, _databaseServer)
            .Add(component => component.HideConnectionInfo, false));

        GetValues().Should().Contain(_databaseServer.Password);
    }

    [When("the database connection information is hidden")]
    public void WhenTheDatabaseConnectionInformationIsHidden()
    {
        GetDatabaseInfoTable().Render(parameters => parameters
            .Add(component => component.dbServer, _databaseServer)
            .Add(component => component.HideConnectionInfo, true));
    }

    [Then("the database connection placeholders should be displayed")]
    public void ThenTheDatabaseConnectionPlaceholdersShouldBeDisplayed()
    {
        GetValues().Should().Contain([
            "<database_host>",
            "<database_name>",
            "<username>",
            "<password>",
            "<port>"
        ]);
    }

    [Then("the database connection values should not be displayed")]
    public void ThenTheDatabaseConnectionValuesShouldNotBeDisplayed()
    {
        GetValues().Should().NotContain([
            _databaseServer.DatabaseHost,
            _databaseServer.DatabaseName,
            _databaseServer.Username,
            _databaseServer.Password,
            _databaseServer.Port
        ]);
    }

    private string[] GetValues()
    {
        var data = GetDatabaseInfoTable().FindComponent<GcdsTable>().Instance.Data;
        return JsonSerializer.SerializeToElement(data)
            .EnumerateArray()
            .Select(row => row.GetProperty("value").GetString() ?? string.Empty)
            .ToArray();
    }

    private IRenderedComponent<DatabaseInfoTable> GetDatabaseInfoTable() =>
        _databaseInfoTable ?? throw new InvalidOperationException("The database information table has not been rendered.");
}
