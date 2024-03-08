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
		Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.Username, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.Password, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestBrowserUrl, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.InfrastructureRepository.MainBranch, Is.Not.Null);

		// assert that Terraform is not null
		Assert.That(_resourceProvisionerConfiguration.Terraform, Is.Not.Null);
		// assert that Backend is not null
		Assert.That(_resourceProvisionerConfiguration.Terraform.Backend, Is.Not.Null);
		// assert each property is not null
		Assert.That(_resourceProvisionerConfiguration.Terraform.Backend.ResourceGroupName, Is.Not.Null);


		// assert that Variables is not null
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables, Is.Not.Null);
		// assert each property is not null
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.azSubscriptionId, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.azTenantId, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.environmentClassification, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.environmentName, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.azLocation, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.resourcePrefix, Is.Not.Null);


		// assert that common_tags is not null
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.commonTags, Is.Not.Null);
		// assert each property is not null
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.commonTags.Sector, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.commonTags.Environment, Is.Not.Null);
		Assert.That(_resourceProvisionerConfiguration.Terraform.Variables.commonTags.ClientOrganization, Is.Not.Null);



	}
}