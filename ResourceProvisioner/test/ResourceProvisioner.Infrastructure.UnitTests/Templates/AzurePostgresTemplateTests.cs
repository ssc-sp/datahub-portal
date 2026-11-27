using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.UnitTests.Collections;
using TerraformVariables = Datahub.Shared.TerraformVariables;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

[NonParallelizable]
[Category("TemplateTests")]
public class AzurePostgresTemplateTests : TemplateTestCollection
{
    private const string PsqlDnsZoneIdVariable = "psql_dns_zone_id";
    private const string PsqlDnsZoneIdConfigKey = $"Terraform:Variables:{PsqlDnsZoneIdVariable}";
    private const string TestPsqlDnsZoneId =
        "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/test-rg/providers/Microsoft.Network/privateDnsZones/test.private.postgres.database.azure.com";

    [SetUp]
    public void RunBeforeEachTest()
    {
        VerifyDirectoryDoesNotExist(DirectoryUtils.GetModuleRepositoryPath(_resourceProvisionerConfiguration));
        VerifyDirectoryDoesNotExist(DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration));
    }

    [TearDown]
    public void RunAfterEachTest()
    {
        _configuration[PsqlDnsZoneIdConfigKey] = null;
    }

    [TestCase(TerraformTemplate.AzurePostgres, false)]
    [TestCase(TerraformTemplate.AzurePostgresEnhanced, false)]
    [TestCase(TerraformTemplate.AzurePostgres, true)]
    [TestCase(TerraformTemplate.AzurePostgresEnhanced, true)]
    public async Task ShouldCopyAndExtractPostgresTemplateFromTheFolderInTheCheckedOutVersion(string templateName,
        bool renamedFolder)
    {
        var workspaceAcronym = await SetupWorkspaceWithModuleVersion();
        if (renamedFolder)
        {
            RenameTemplateFolder(templateName);
        }

        _configuration[PsqlDnsZoneIdConfigKey] = TestPsqlDnsZoneId;
        var command = GenerateTestWorkspaceDefinition(workspaceAcronym, [TerraformTemplate.NewProjectTemplate]);

        await _terraformService.CopyTemplateAsync(templateName, command);
        await _terraformService.ExtractVariables(templateName, command);

        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);
        var templatePath = DirectoryUtils.GetTemplatePath(_resourceProvisionerConfiguration, templateName);
        var templateTerraformFiles = Directory.GetFiles(templatePath, "*.tf", SearchOption.TopDirectoryOnly);
        var variables = await ReadVariables(projectPath, templateName);

        Assert.Multiple(() =>
        {
            Assert.That(templateTerraformFiles, Is.Not.Empty);
            foreach (var file in templateTerraformFiles)
            {
                Assert.That(File.Exists(Path.Join(projectPath, Path.GetFileName(file))), Is.True,
                    $"{Path.GetFileName(file)} was not copied to the workspace");
            }

            Assert.That(variables[TerraformVariables.PsqlSku]?.GetValue<string>(), Is.EqualTo("B_Standard_B1ms"));
        });
    }

    [Test]
    public async Task ShouldWritePsqlDnsZoneIdFromConfigurationForEnhancedPostgres()
    {
        var workspaceAcronym = await SetupWorkspaceWithModuleVersion();
        _configuration[PsqlDnsZoneIdConfigKey] = TestPsqlDnsZoneId;
        var command = GenerateTestWorkspaceDefinition(workspaceAcronym, [TerraformTemplate.NewProjectTemplate]);

        await _terraformService.CopyTemplateAsync(TerraformTemplate.AzurePostgresEnhanced, command);
        await _terraformService.ExtractVariables(TerraformTemplate.AzurePostgresEnhanced, command);

        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);
        var variables = await ReadVariables(projectPath, TerraformTemplate.AzurePostgresEnhanced);

        Assert.That(variables[PsqlDnsZoneIdVariable]?.GetValue<string>(), Is.EqualTo(TestPsqlDnsZoneId));
    }

    [Test]
    public async Task ShouldThrowWhenPsqlDnsZoneIdIsNotConfiguredForEnhancedPostgres()
    {
        var workspaceAcronym = await SetupWorkspaceWithModuleVersion();
        _configuration[PsqlDnsZoneIdConfigKey] = null;
        var command = GenerateTestWorkspaceDefinition(workspaceAcronym, [TerraformTemplate.NewProjectTemplate]);

        await _terraformService.CopyTemplateAsync(TerraformTemplate.AzurePostgresEnhanced, command);

        var exception = await Assert.ThrowsAsync<MissingTerraformVariableException>(async () =>
            await _terraformService.ExtractVariables(TerraformTemplate.AzurePostgresEnhanced, command));
        Assert.That(exception!.Message, Does.Contain(PsqlDnsZoneIdVariable));
    }

    /// <summary>
    /// Sets up a workspace and checks out the module repository at the test workspace version, which still uses the
    /// original postgres template folder names.
    /// </summary>
    private static async Task<string> SetupWorkspaceWithModuleVersion()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        await SetupNewProjectTemplate(workspaceAcronym);
        await _repositoryService.FetchModuleRepository(TestWorkspaceVersion);
        return workspaceAcronym;
    }

    /// <summary>
    /// Renames a template folder in the checked-out module repository to its new name, as in later module versions.
    /// </summary>
    private static void RenameTemplateFolder(string templateName)
    {
        var templatesPath = Path.Join(DirectoryUtils.GetModuleRepositoryPath(_resourceProvisionerConfiguration),
            _resourceProvisionerConfiguration.ModuleRepository.TemplatePathPrefix);
        var originalFolder = Path.Join(templatesPath, templateName);
        var renamedFolder = Path.Join(templatesPath, TerraformTemplate.RenamedTemplateFolders[templateName]);

        Assert.That(Directory.Exists(originalFolder), Is.True,
            $"Expected {templateName} folder at module version {TestWorkspaceVersion}");
        Directory.Move(originalFolder, renamedFolder);
    }

    private static async Task<JsonObject> ReadVariables(string projectPath, string templateName)
    {
        var variablesFile = Path.Join(projectPath, $"{templateName}.auto.tfvars.json");
        Assert.That(File.Exists(variablesFile), Is.True, $"{variablesFile} was not written");
        return JsonSerializer.Deserialize<JsonObject>(await File.ReadAllTextAsync(variablesFile))!;
    }
}
