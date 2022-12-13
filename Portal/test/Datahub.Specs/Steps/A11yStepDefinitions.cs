using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Datahub.Specs.PageObjects;
using TechTalk.SpecFlow;

namespace Datahub.Specs.Steps;

[Binding]
// ReSharper disable once InconsistentNaming
public sealed class A11yStepDefinitions
{

    private readonly HomePageObject _homePageObject;

    public A11yStepDefinitions(HomePageObject homePageObject)
    {
        _homePageObject = homePageObject;
    }

    [Given(@"the user is on the home page")]
    public async Task GivenTheUserIsOnTheHomePage()
    {
        await _homePageObject.NavigateAsync();
    }

    [Then(@"there should be no accessibility errors")]
    public void ThenThereShouldBeNoAccessibilityErrors()
    {
        // ScenarioContext.StepIsPending();
    }

}