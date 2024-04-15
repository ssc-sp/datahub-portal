using Datahub.Specs.PageObjects;
using Reqnroll;

namespace Datahub.Specs.Steps;

[Binding]
public sealed class AuthenticationStepDefinitions
{
    private readonly LoginPageObject _loginPageObject;

    public AuthenticationStepDefinitions(LoginPageObject loginPageObject)
    {
        _loginPageObject = loginPageObject;
    }

    [Given(@"the user is authenticated")]
    public async Task GivenTheUserIsAuthenticated()
    {
        await _loginPageObject.LoginAsync();
    }
}