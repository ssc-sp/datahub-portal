using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceProvisioner.Application.Config;
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

        [Given(@"service principal credentials are available")]
        public void GivenServicePrincipalCredentialsAreAvailable()
        {
            var resourceProvisionerConfiguration = _scenarioContext.Get<ResourceProvisionerConfiguration>();
            
            Assert.NotNull(resourceProvisionerConfiguration);
            Assert.NotNull(resourceProvisionerConfiguration.InfrastructureRepository);
            Assert.NotNull(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration);
            Assert.NotNull(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ClientId);
            Assert.NotNull(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ClientSecret);
            Assert.NotNull(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.TenantId);
        }
    }
}