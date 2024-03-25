using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Xunit;

namespace ResourceProvisioner.SpecflowTests.Steps
{
    [Binding]
    public sealed class AzureDevopsGitWithAccessTokenStepDefinitions
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;

        public AzureDevopsGitWithAccessTokenStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }


        [Given(@"a service principal")]
        public void GivenAServicePrincipal()
        {
            // load service principal from configuration
            
            
            
        }
    }
}