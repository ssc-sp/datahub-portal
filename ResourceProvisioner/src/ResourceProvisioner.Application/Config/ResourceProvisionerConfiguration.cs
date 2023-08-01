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
    public string Url { get; set; }
    public string LocalPath { get; set; }
    public string TemplatePathPrefix { get; set; }
    public string ModulePathPrefix { get; set; }
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
    public string az_subscription_id { get; set; }
    public string az_tenant_id { get; set; }
    public string budget_amount { get; set; }
    public string storage_size_limit_tb { get; set; }
    public string environment_classification { get; set; }
    public string environment_name { get; set; }
    public string az_location { get; set; }
    public string resource_prefix { get; set; }
    public string datahub_app_sp_oid { get; set; }
    public string azure_databricks_enterprise_oid { get; set; }
    public string log_workspace_id { get; set; }
    public string aad_admin_group_oid { get; set; }
    public CommonTags common_tags { get; set; }
}

