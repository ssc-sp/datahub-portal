using FluentAssertions;
using TechTalk.SpecFlow;

namespace Datahub.UI.Tests;

[Binding]
public class TranslationSteps
{
    private readonly HomePageObject _homePageObject;

    public TranslationSteps(HomePageObject homePageObject)
    {
        _homePageObject= homePageObject;
    }

    [Given(@"I am an authenticated user navigating on the home page")]
    public async Task GivenAUserOnHomePage()
    {

    }
}

