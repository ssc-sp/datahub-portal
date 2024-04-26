using System.IdentityModel.Tokens.Jwt;
using Azure.Core;
using Datahub.Shared.Clients;
using Reqnroll;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Common;
using Xunit;

namespace ResourceProvisioner.SpecflowTests.Steps
{
    [Binding]
    public sealed class AzureDevopsGitWithAccessTokenStepDefinitions(
        ScenarioContext scenarioContext,
        ResourceProvisionerConfiguration resourceProvisionerConfiguration,
        IRepositoryService repositoryService)
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        [Given(@"service principal credentials are available")]
        public void GivenServicePrincipalCredentialsAreAvailable()
        {
            Assert.NotNull(resourceProvisionerConfiguration);
            Assert.NotNull(resourceProvisionerConfiguration.InfrastructureRepository);
            Assert.NotNull(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration);
            Assert.NotNull(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ClientId);
            Assert.NotNull(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ClientSecret);
            Assert.NotNull(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.TenantId);
        }

        [When(@"it requests an access token")]
        public async Task WhenItRequestsAnAccessToken()
        {
            var azureDevOpsClient = new AzureDevOpsClient(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration);
            var accessToken = await azureDevOpsClient.AccessTokenAsync();
            
            scenarioContext["accessToken"] = accessToken;
        }

        [Then(@"it should get a valid access token")]
        public void ThenItShouldGetAValidAccessToken()
        {
            var accessToken = scenarioContext["accessToken"] as AccessToken? ?? default;
            
            // Check if the token is valid
            Assert.NotNull(accessToken.Token);
            Assert.NotEmpty(accessToken.Token);
            
            // Check if the token is a valid JWT token
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(accessToken.Token);
            Assert.NotNull(token);
            
            // Check if the token is not expired
            Assert.True(token.ValidTo > DateTime.UtcNow);
            
            // Check if the token is not issued in the future
            Assert.True(token.ValidFrom < DateTime.UtcNow);
            
            // Check if the token is from the correct issuer
            Assert.Equal("https://sts.windows.net/" + resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.TenantId + "/", token.Issuer);
            
            // Check if the token is for the correct audience
            Assert.Equal(AzureDevOpsClient.AzureDevopsScope, token.Audiences.First());
        }
        
        [Given(@"the cloned repository does not exist")]
        public void GivenTheClonedRepositoryDoesNotExist()
        {
            var expectedClonePath = Path.Join(Environment.CurrentDirectory, resourceProvisionerConfiguration.InfrastructureRepository.LocalPath);
            if (Directory.Exists(expectedClonePath))
            {
                DirectoryUtils.VerifyDirectoryDoesNotExist(expectedClonePath);
            }
            Assert.False(Directory.Exists(expectedClonePath));
        }

        [When(@"it tries to clone Azure Devops Git repository")]
        public async Task WhenItTriesToCloneAzureDevopsGitRepository()
        {
            await repositoryService.FetchInfrastructureRepository();
        }

        [Then(@"the cloned repository should exist")]
        public void ThenTheClonedRepositoryShouldExist()
        {
            var expectedClonePath = Path.Join(Environment.CurrentDirectory, resourceProvisionerConfiguration.InfrastructureRepository.LocalPath);
            Assert.True(Directory.Exists(expectedClonePath));
        }
    }
}