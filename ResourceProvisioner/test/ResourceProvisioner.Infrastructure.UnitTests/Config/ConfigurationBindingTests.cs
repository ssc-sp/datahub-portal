namespace ResourceProvisioner.Infrastructure.UnitTests.Config;

using static Testing;
public class ConfigurationBindingTests
{
    [SetUp]
    public void RunBeforeEachTest()
    {
    }

    [Test]
    public void ShouldProperlyBindToModuleRepository()
    {
        // assert that it's not null
        Assert.That(_resourceProvisionerConfiguration, Is.Not.Null);
        
        // assert that ModuleRepository is not null
        Assert.That(_resourceProvisionerConfiguration.ModuleRepository, Is.Not.Null);
        // assert each property is not null
        Assert.That(_resourceProvisionerConfiguration.ModuleRepository.Url, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.ModuleRepository.LocalPath, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.ModuleRepository.TemplatePathPrefix, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.ModuleRepository.ModulePathPrefix, Is.Not.Null);
        
        // assert that InfrastructureRepository is not null
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository, Is.Not.Null);
        // assert each property is not null
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.Url, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.LocalPath, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.ProjectPathPrefix, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestBrowserUrl, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.MainBranch, Is.Not.Null);
        
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ClientId, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ClientSecret, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.OrganizationName, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ProjectName, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.TenantId, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.AppServiceConfigPipeline, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ListPipelineUrlTemplate, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.PostPipelineRunUrlTemplate, Is.Not.Null);
        
        // assert that Terraform is not null
        Assert.That(_resourceProvisionerConfiguration.Terraform, Is.Not.Null);
        // assert that Backend is not null
        Assert.That(_resourceProvisionerConfiguration.Terraform.Backend, Is.Not.Null);
        // assert each property is not null
        Assert.That(_resourceProvisionerConfiguration.Terraform.Backend.ResourceGroupName, Is.Not.Null);
        
        
        // assert that Variables is not null
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables, Is.Not.Null);
        // assert each property is not null
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.az_subscription_id, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.az_tenant_id, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.environment_classification, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.environment_name, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.az_location, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.resource_prefix, Is.Not.Null);
        
        
        // assert that common_tags is not null
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.common_tags, Is.Not.Null);
        // assert each property is not null
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.common_tags.Sector, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.common_tags.Environment, Is.Not.Null);
        Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.common_tags.ClientOrganization, Is.Not.Null);
        
        
        
    }
}