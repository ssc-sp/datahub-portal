using Datahub.Shared.Entities;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Infrastructure.Common;

namespace ResourceProvisioner.Infrastructure.UnitTests;

public class DirectoryUtilsTests
{
    [Test]
    public void ShouldThrowExceptionWhenTemplateNameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            DirectoryUtils.GetTemplatePath(new ResourceProvisionerConfiguration(), null);
        });
    }

    [Test]
    public void ShouldThrowExceptionWhenAcronymIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            DirectoryUtils.GetProjectPath(new ResourceProvisionerConfiguration(), null);
        });
    }

    [TestCase(TerraformTemplate.AzurePostgres, "azure-postgres")]
    [TestCase(TerraformTemplate.AzurePostgresEnhanced, "azure-postgres-enhanced")]
    [TestCase(TerraformTemplate.AzurePostgres, "azure-postgresql")]
    [TestCase(TerraformTemplate.AzurePostgresEnhanced, "azure-postgresql-enhanced")]
    [TestCase(TerraformTemplate.AzureDatabricks, "azure-databricks")]
    public void ShouldGetTemplatePathOfTheFolderInTheCheckedOutVersion(string templateName, string folderInModuleVersion)
    {
        var configuration = CreateModuleRepositoryConfiguration();
        var templatesPath = GetTemplatesPath(configuration);
        try
        {
            Directory.CreateDirectory(Path.Join(templatesPath, folderInModuleVersion));

            var templatePath = DirectoryUtils.GetTemplatePath(configuration, templateName);

            Assert.That(templatePath, Is.EqualTo(Path.Join(templatesPath, folderInModuleVersion)));
        }
        finally
        {
            DeleteModuleRepository(configuration);
        }
    }

    [Test]
    public void ShouldGetTemplatePathOfTheTemplateNameWhenNoFolderExists()
    {
        var configuration = CreateModuleRepositoryConfiguration();
        var templatesPath = GetTemplatesPath(configuration);
        try
        {
            Directory.CreateDirectory(templatesPath);

            var templatePath = DirectoryUtils.GetTemplatePath(configuration, TerraformTemplate.AzurePostgres);

            Assert.That(templatePath, Is.EqualTo(Path.Join(templatesPath, TerraformTemplate.AzurePostgres)));
        }
        finally
        {
            DeleteModuleRepository(configuration);
        }
    }

    private static ResourceProvisionerConfiguration CreateModuleRepositoryConfiguration() => new()
    {
        ModuleRepository = new ModuleRepositoryConfiguration
        {
            LocalPath = $"directory-utils-tests-{Guid.NewGuid():N}",
            TemplatePathPrefix = "templates/"
        }
    };

    private static string GetTemplatesPath(ResourceProvisionerConfiguration configuration) =>
        Path.Join(DirectoryUtils.GetModuleRepositoryPath(configuration), configuration.ModuleRepository.TemplatePathPrefix);

    private static void DeleteModuleRepository(ResourceProvisionerConfiguration configuration)
    {
        var localPath = Path.Join(Environment.CurrentDirectory, configuration.ModuleRepository.LocalPath);
        if (Directory.Exists(localPath))
        {
            Directory.Delete(localPath, true);
        }
    }
}
