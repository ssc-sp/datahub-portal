using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace Datahub.Specs.Steps
{
    [Binding]
    public sealed class AuthenticationStepDefinitions
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;

        public AuthenticationStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"the user is authenticated")]
        public void GivenTheUserIsAuthenticated()
        {
        }
    }
}