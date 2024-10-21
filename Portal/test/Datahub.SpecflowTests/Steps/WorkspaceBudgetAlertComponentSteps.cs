using System.Globalization;
using Bunit;
using Datahub.Application.Configuration;
using Datahub.Infrastructure.Offline;
using Datahub.Portal.Pages.Workspace;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Reqnroll;
using Xunit;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public class WorkspaceBudgetAlertComponentSteps(
    ScenarioContext scenarioContext,
    IWebHostEnvironment hostingEnvironment
) : TestContext
{
    private const string RelativePathToSrc = "../../../../../src";

    [Given(@"there is a workspace budget alert component with a percent budget of (.*)")]
    public void GivenThereIsAWorkspaceBudgetAlertComponentWithAPercentBudgetOf(decimal percent)
    {
        Services.AddSingleton<IWebHostEnvironment>(hostingEnvironment);
        var portalConfiguration = new DatahubPortalConfiguration()
        {
            CultureSettings =
            {
                ResourcesPath = $"{RelativePathToSrc}/Datahub.Portal/i18n",
                AdditionalResourcePaths = []
            }
        };
        Services.AddDatahubOfflineInfrastructureServices(portalConfiguration);
        Services.AddMudLocalization();

        var workspaceBudgetAlert = RenderComponent<WorkspaceBudgetAlert>(opt =>
            opt.Add(p => p.PercentBudgetSpent, percent));

        scenarioContext["workspaceBudgetAlert"] = workspaceBudgetAlert;
    }

    [Then(@"the alert should not be rendered")]
    public void ThenTheAlertShouldNotBeRendered()
    {
        var workspaceBudgetAlert = scenarioContext["workspaceBudgetAlert"] as IRenderedComponent<WorkspaceBudgetAlert>;

        Assert.Throws<ElementNotFoundException>(() => { workspaceBudgetAlert!.Find(".mud-alert"); });
    }

    [Then(@"the alert should be rendered with (.*) budget and (.*) class")]
    public void ThenTheAlertShouldBeRenderedWithBudgetAndClass(decimal percent, string classname)
    {
        var workspaceBudgetAlert = scenarioContext["workspaceBudgetAlert"] as IRenderedComponent<WorkspaceBudgetAlert>;

        var alert = workspaceBudgetAlert!.Find(".mud-alert");
        alert.ClassList.Should().Contain(classname);
        alert.TextContent.Should().Contain(percent.ToString(CultureInfo.InvariantCulture));
    }
}