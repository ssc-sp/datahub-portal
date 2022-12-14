using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Datahub.Specs.PageObjects;
using FluentAssertions;
using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Playwright.Axe;

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
        _homePageObject.Page.Url.Should().EndWith("/home");
    }

    [Then(@"there should be no accessibility errors")]
    public async Task ThenThereShouldBeNoAccessibilityErrors()
    {
        await _homePageObject.Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        var axeResults = await _homePageObject.Page.RunAxe();
        axeResults.Violations.Should().BeEmpty();
    }

}