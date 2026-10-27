using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Datahub.Shared.Configuration;

namespace ResourceProvisioner.Application.Config;

public class Backend
{
    public string ResourceGroupName { get; set; }
    public string SubscriptionId { get; set; }
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
    public string Name { get; set; }
    public string ProjectPathPrefix { get; set; }
    public string PullRequestUrl { get; set; }
    public string PullRequestBrowserUrl { get; set; }
    public bool EnablePullRequestAutoComplete { get; set; } = true;
    public string ApiVersion { get; set; } = "7.1-preview.1";
    public string MainBranch { get; set; }
    public AzureDevOpsConfiguration AzureDevOpsConfiguration { get; set; } = new();
}

public class ModuleRepositoryConfiguration
{
    public const string DefaultBranch = "main";
    public string Url { get; set; }
    public string LocalPath { get; set; }

    public string Name = "datahub-resource-modules";
    public string TemplatePathPrefix { get; set; }
    public string ModulePathPrefix { get; set; } = "modules/";

    public string Branch { get; set; } = "dev";
}

public class ResourceProvisionerConfiguration
{
    [Required]
    public ModuleRepositoryConfiguration ModuleRepository { get; set; } = null!;
    [Required]
    public InfrastructureRepositoryConfiguration InfrastructureRepository { get; set; } = null!;
    [Required]
    public TerraformConfiguration Terraform { get; set; } = null!;
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
    public string service_bus_id { get; set; } = string.Empty;
    public string log_analytics_workspace_id { get; set; } = string.Empty;
    public string automation_account_uai_name { get; set; } = string.Empty;
    public string automation_account_uai_rg { get; set; } = string.Empty;
    public string automation_account_uai_sub { get; set; } = string.Empty;
    public string az_subscription_id { get; set; } = string.Empty;
    public string az_tenant_id { get; set; } = string.Empty;
    public string budget_amount { get; set; } = string.Empty;
    public string storage_size_limit_tb { get; set; } = string.Empty;
    public string environment_classification { get; set; } = string.Empty;
    public string environment_name { get; set; } = string.Empty;
    public string az_location { get; set; } = string.Empty;
    public string allow_source_ip { get; set; } = string.Empty;
    public string resource_prefix { get; set; } = string.Empty;
    public string resource_prefix_alphanumeric { get; set; } = string.Empty;
    public string storage_suffix { get; set; } = "terraformbackend";
    public string datahub_app_sp_oid { get; set; } = string.Empty;
    public string azure_databricks_enterprise_oid { get; set; } = string.Empty;
    public string log_workspace_id { get; set; } = string.Empty;
    public string aad_admin_group_oid { get; set; } = string.Empty;

    public string ssc_cbrid { get; set; } = string.Empty;
    public CommonTags common_tags { get; set; } = new();
}
