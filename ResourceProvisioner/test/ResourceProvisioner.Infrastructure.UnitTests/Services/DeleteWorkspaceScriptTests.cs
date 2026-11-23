using Datahub.Shared.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure.UnitTests.Services;

/// <summary>
/// The delete.ps1 script written when a workspace is deleted, which the infrastructure pipeline runs.
/// </summary>
[NonParallelizable]
public class DeleteWorkspaceScriptTests
{
    private const string Acronym = "TST";
    private const string SubscriptionId = "00000000-0000-0000-0000-000000000001";
    private const string WorkspaceResourceGroup = "fsdh_proj_tst_dev_rg";

    private ResourceProvisionerConfiguration _configuration = null!;
    private string _projectPath = null!;
    private string _previousTempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _previousTempDirectory = DirectoryUtils.tempDirectory;
        DirectoryUtils.tempDirectory = Guid.NewGuid().ToString("N")[..8];

        _configuration = new ResourceProvisionerConfiguration
        {
            InfrastructureRepository = new InfrastructureRepositoryConfiguration
            {
                LocalPath = "delete-script-tests",
                Name = "infra",
                ProjectPathPrefix = "terraform/projects"
            },
            Terraform = new TerraformConfiguration
            {
                Variables = new Variables { resource_prefix = "FSDH", environment_name = "dev" }
            }
        };
        _projectPath = DirectoryUtils.GetProjectPath(_configuration, Acronym);
        Directory.CreateDirectory(_projectPath);
    }

    [TearDown]
    public void TearDown()
    {
        var root = Path.Join(Environment.CurrentDirectory, _configuration.InfrastructureRepository.LocalPath);
        if (Directory.Exists(root))
        {
            Directory.Delete(root, true);
        }

        DirectoryUtils.tempDirectory = _previousTempDirectory;
    }

    [Test]
    public async Task DeletesTheDatabricksManagedResourceGroupAfterTheWorkspaceResourceGroup_OnlyIfItExistsAndBelongsToTheWorkspace()
    {
        await CreateTerraformService().DeleteWorkspaceAsync(Workspace(), WorkspaceResourceGroup);

        // Same name as the azure-databricks module: lower("<resource_prefix>-dbk-<project_cd>-<environment_name>-rg")
        const string managedResourceGroup = "fsdh-dbk-tst-dev-rg";
        Assert.That(ReadScriptLines(), Is.EqualTo(new[]
        {
            $"az account set --subscription {SubscriptionId}",
            $"az group delete --name {WorkspaceResourceGroup} --yes",
            $"if ((az group exists --name {managedResourceGroup}) -eq \"true\") {{",
            $"  $managedBy = az group show --name {managedResourceGroup} --query managedBy --output tsv",
            $"  if ($managedBy -like '/subscriptions/{SubscriptionId}/resourceGroups/{WorkspaceResourceGroup}/providers/Microsoft.Databricks/workspaces/*') {{",
            $"    az group delete --name {managedResourceGroup} --yes",
            "  } else {",
            $"    Write-Host \"Not deleting {managedResourceGroup}: it is managed by '$managedBy', not a Databricks workspace in {WorkspaceResourceGroup}\"",
            "  }",
            "}"
        }));
    }

    [TestCase("", "dev")]
    [TestCase("fsdh", "")]
    public async Task WithoutResourcePrefixOrEnvironmentName_OnlyDeletesTheWorkspaceResourceGroup(string resourcePrefix,
        string environmentName)
    {
        _configuration.Terraform.Variables.resource_prefix = resourcePrefix;
        _configuration.Terraform.Variables.environment_name = environmentName;

        await CreateTerraformService().DeleteWorkspaceAsync(Workspace(), WorkspaceResourceGroup);

        Assert.That(ReadScriptLines(), Is.EqualTo(new[]
        {
            $"az account set --subscription {SubscriptionId}",
            $"az group delete --name {WorkspaceResourceGroup} --yes"
        }));
    }

    [TestCase("g2dc-cto-fsdh", "TST", "dev", ExpectedResult = "g2dc-cto-fsdh-dbk-tst-dev-rg")]
    [TestCase("G2DC-CTO-FSDH", "NPT21", "PRD", ExpectedResult = "g2dc-cto-fsdh-dbk-npt21-prd-rg")]
    [TestCase("fsdh", "abc", "pre", ExpectedResult = "fsdh-dbk-abc-pre-rg")]
    public string DatabricksManagedResourceGroupName_FollowsTheModuleNaming(string resourcePrefix, string acronym,
        string environmentName) =>
        TerraformService.GetDatabricksManagedResourceGroupName(resourcePrefix, acronym, environmentName);

    [Test]
    public void DeleteDatabricksManagedResourceGroupLines_PutEachValueInItsPlace()
    {
        var lines = TerraformService.DeleteDatabricksManagedResourceGroupLines("sub-1", "ws_rg", "managed-rg");

        Assert.That(lines, Is.EqualTo(new[]
        {
            "if ((az group exists --name managed-rg) -eq \"true\") {",
            "  $managedBy = az group show --name managed-rg --query managedBy --output tsv",
            "  if ($managedBy -like '/subscriptions/sub-1/resourceGroups/ws_rg/providers/Microsoft.Databricks/workspaces/*') {",
            "    az group delete --name managed-rg --yes",
            "  } else {",
            "    Write-Host \"Not deleting managed-rg: it is managed by '$managedBy', not a Databricks workspace in ws_rg\"",
            "  }",
            "}"
        }));
    }

    private const string OwnWorkspace =
        "/subscriptions/sub-1/resourceGroups/fsdh_proj_tst_dev_rg/providers/Microsoft.Databricks/workspaces/fsdh-dbk-tst-dev";

    /// <summary>
    /// Runs the generated lines in PowerShell with a fake az command, to check what the script really deletes.
    /// </summary>
    [TestCase(true, OwnWorkspace, ExpectedResult = true, TestName = "Deletes when it exists and is managed by the workspace's Databricks")]
    [TestCase(true, "/subscriptions/sub-1/resourcegroups/FSDH_PROJ_TST_DEV_RG/providers/microsoft.databricks/workspaces/x", ExpectedResult = true, TestName = "Deletes when Azure returns managedBy in another casing")]
    [TestCase(false, OwnWorkspace, ExpectedResult = false, TestName = "Skips when the group doesn't exist")]
    [TestCase(true, "", ExpectedResult = false, TestName = "Skips when the group isn't managed by anything")]
    [TestCase(true, "/subscriptions/sub-1/resourceGroups/fsdh_proj_abc_dev_rg/providers/Microsoft.Databricks/workspaces/x", ExpectedResult = false, TestName = "Skips when managed by another workspace's Databricks")]
    [TestCase(true, "/subscriptions/sub-1/resourceGroups/fsdh_proj_tst_dev_rg_old/providers/Microsoft.Databricks/workspaces/x", ExpectedResult = false, TestName = "Skips when managed from a resource group whose name only starts the same")]
    [TestCase(true, "/subscriptions/sub-2/resourceGroups/fsdh_proj_tst_dev_rg/providers/Microsoft.Databricks/workspaces/x", ExpectedResult = false, TestName = "Skips when managed from another subscription")]
    [TestCase(true, "/subscriptions/sub-1/resourceGroups/fsdh_proj_tst_dev_rg/providers/Microsoft.Storage/storageAccounts/x", ExpectedResult = false, TestName = "Skips when managed by something other than Databricks")]
    public bool DeleteDatabricksManagedResourceGroupLines_InPowerShell(bool groupExists, string managedBy)
    {
        var fakeAz = new[]
        {
            "function az {",
            $"  if ($args[1] -eq 'exists') {{ return '{(groupExists ? "true" : "false")}' }}",
            $"  if ($args[1] -eq 'show') {{ return '{managedBy}' }}",
            "  if ($args[1] -eq 'delete') { Write-Output \"DELETED $($args[3])\" }",
            "}"
        };
        var lines = TerraformService.DeleteDatabricksManagedResourceGroupLines("sub-1", WorkspaceResourceGroup,
            "fsdh-dbk-tst-dev-rg");

        var output = RunPowerShell(fakeAz.Concat(lines));

        return output.Contains("DELETED fsdh-dbk-tst-dev-rg");
    }

    private static string RunPowerShell(IEnumerable<string> scriptLines)
    {
        var scriptPath = Path.Join(Path.GetTempPath(), $"delete-dbk-{Guid.NewGuid():N}.ps1");
        File.WriteAllText(scriptPath, string.Join("\n", scriptLines));
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo("pwsh",
                ["-NoProfile", "-NonInteractive", "-File", scriptPath])
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            System.Diagnostics.Process process;
            try
            {
                process = System.Diagnostics.Process.Start(startInfo)!;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Assert.Ignore("PowerShell (pwsh) isn't installed");
                throw;
            }

            using (process)
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Assert.That(error, Is.Empty, "The script wrote errors");
                return output;
            }
        }
        finally
        {
            File.Delete(scriptPath);
        }
    }

    private TerraformService CreateTerraformService() => new(NullLogger<TerraformService>.Instance,
        Options.Create(_configuration), new ConfigurationBuilder().Build());

    private static TerraformWorkspace Workspace() => new() { Acronym = Acronym, SubscriptionId = SubscriptionId };

    private string[] ReadScriptLines() => File.ReadAllText(Path.Join(_projectPath, "delete.ps1")).Split('\n');
}
