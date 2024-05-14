using Datahub.Specs.PageObjects;
using Reqnroll;

namespace Datahub.Specs.Steps
{
    [Binding]
    public class LoginStepDefinitions
    {
        
        private readonly LoginPageObject _loginPageObject;
        
        public LoginStepDefinitions(LoginPageObject loginPageObject)
        {
            _loginPageObject = loginPageObject;
        }
        
        [Given(@"an unregistered user navigates to the login page")]
        public async Task GivenAnUnregisteredUserNavigatesToTheLoginPage()
        {
            await _loginPageObject.NavigateAsync();
        }

        [Then(@"the login page is accessible")]
        public async Task ThenTheLoginPageIsAccessible()
        {
            await _loginPageObject.ValidateLocationAsync();
        }
    }
}