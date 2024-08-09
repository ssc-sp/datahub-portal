using System.Text.Json.Nodes;
using Datahub.Shared.Configuration;

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
    public string PullRequestUrl { get; set; }
    public string PullRequestBrowserUrl { get; set; }
    public string ApiVersion { get; set; } = "7.1-preview.1";
    public string MainBranch { get; set; }
    public AzureDevOpsConfiguration AzureDevOpsConfiguration { get; set; } = new(); 
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
    public string automation_account_uai_name { get; set; }
    public string automation_account_uai_rg { get; set; }
    public string automation_account_uai_sub { get; set; }
    public string az_subscription_id { get; set; }
    public string az_tenant_id { get; set; }
    public string budget_amount { get; set; }
    public string storage_size_limit_tb { get; set; }
    public string environment_classification { get; set; }
    public string environment_name { get; set; }
    public string az_location { get; set; }
    public string allow_source_ip { get; set; }
    public string resource_prefix { get; set; }
    public string resource_prefix_alphanumeric { get; set; }
    public string storage_suffix { get; set; } = "terraformbackend";
    public string datahub_app_sp_oid { get; set; }
    public string azure_databricks_enterprise_oid { get; set; }
    public string log_workspace_id { get; set; } = "";
    public string aad_admin_group_oid { get; set; }
    public CommonTags common_tags { get; set; }
}

