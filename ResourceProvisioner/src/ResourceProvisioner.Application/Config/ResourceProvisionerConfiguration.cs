using System.Text.Json.Nodes;

namespace ResourceProvisioner.Application.Config;

public class Backend
{
    public string ResourceGroupName { get; set; }
}

public class CommonTags
{
    public string Sector { get; set; }
    public string Environment { get; set; }
    public string ClientOrganization { get; set; }
}

public class InfrastructureRepositoryConfiguration
{
    public string Url { get; set; }
    public string LocalPath { get; set; }
    public string ProjectPathPrefix { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string PullRequestUrl { get; set; }
    public string PullRequestBrowserUrl { get; set; }
    public string ApiVersion { get; set; }
    public string MainBranch { get; set; }
    public string AutoApproveUserOid { get; set; }
}

public class ModuleRepositoryConfiguration
{
    public const string DefaultBranch = "main";
    public string Url { get; set; }
    public string LocalPath { get; set; }
    public string TemplatePathPrefix { get; set; }
    public string ModulePathPrefix { get; set; } = "modules/";

    public string Branch { get; set; } = "main";
}

public class ResourceProvisionerConfiguration
{
    public ModuleRepositoryConfiguration ModuleRepository { get; set; }
    public InfrastructureRepositoryConfiguration InfrastructureRepository { get; set; }
    public TerraformConfiguration Terraform { get; set; }
}

public class TerraformConfiguration
{
    public Backend Backend { get; set; }
    public Variables Variables { get; set; }

}

public class OmniUser
{
    public string email { get; set; }
    public string oid { get; set; }

    public JsonObject ToJsonObject()
    {
        return new JsonObject()
        {
            ["email"] = email,
            ["oid"] = oid,
        };
    }
}

public class Variables
{
    public string azSubscriptionId { get; set; }
    public string azTenantId { get; set; }
    public string budgetAmount { get; set; }
    public string storageSizeLimitTb { get; set; }
    public string environmentClassification { get; set; }
    public string environmentName { get; set; }
    public string azLocation { get; set; }
    public string resourcePrefix { get; set; }
    public string datahubAppSpOid { get; set; }
    public string azureDatabricksEnterpriseOid { get; set; }
    public string logWorkspaceId { get; set; } = "";
    public string aadAdminGroupOid { get; set; }
    public CommonTags commonTags { get; set; }
}

