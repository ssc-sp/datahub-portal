using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;

namespace Datahub.Core.Extensions
{
    public static class ProjectResourceGroupExtension
    {
        public static string? GetResourceGroupName(this Datahub_Project project)
        {
            var newProjectTemplateType =
                TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate);
            var newProjectResource = project.Resources?.FirstOrDefault(r => r.ResourceType == newProjectTemplateType);
            if (newProjectResource is not null && newProjectResource.CreatedAt.HasValue)
            {
                if (string.IsNullOrEmpty(newProjectResource.JsonContent))
                {
                    throw new Exception("Resource group name not found");
                }
                var jsonContent = JsonSerializer.Deserialize<JsonObject>(newProjectResource.JsonContent);
                string rgName = jsonContent?["resource_group_name"]?.ToString() ?? throw new Exception("Resource group name not found");
                if (rgName == "Missing") throw new Exception("Resource group name not found");
                return rgName;
            }
            return null;
        }

        public static string GetPostgresId(this Project_Resources2 postgresResource)
        {
            if (postgresResource is not null && postgresResource.CreatedAt.HasValue && TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres) == postgresResource.ResourceType)
            {
                if (string.IsNullOrEmpty(postgresResource.JsonContent))
                {
                    throw new Exception("Resource group name not found");
                }
                var jsonContent = JsonSerializer.Deserialize<JsonObject>(postgresResource.JsonContent)!;
                string id = jsonContent["postgres_id"]!.ToString();
                if (id == "Missing") throw new Exception("Postgres ID not found");
                return id;
            }
            throw new Exception("Postgres ID not found");
        }

        public static string GetResourceGroupNameFromBlob(this Datahub_Project project)
        {
            var blobStorageTemplateType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
            var blobStorageResource = project.Resources?.FirstOrDefault(r => r.ResourceType == blobStorageTemplateType);
            if (blobStorageResource is not null && blobStorageResource.CreatedAt.HasValue)
            {
                if (string.IsNullOrEmpty(blobStorageResource.JsonContent))
                {
                    throw new Exception("Resource group name not found");
                }
                var jsonContent = JsonSerializer.Deserialize<JsonObject>(blobStorageResource.JsonContent)!;
                var rgName = jsonContent["resource_group_name"]!.ToString();
                if (rgName == "Missing") throw new Exception("Resource group name not found");
                return rgName;
            }

            throw new Exception("Resource group name not found");
        }

        public static string GetDbkResourceGroupName(this Datahub_Project project, string projectResourceGroupName)
        {
            var dbkResourceTemplateType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
            var dbkResource = project.Resources?.FirstOrDefault(r => r.ResourceType == dbkResourceTemplateType);
            if (dbkResource is not null && dbkResource.CreatedAt.HasValue)
            {
                string rgName = string.Join(
                    "-",
                    projectResourceGroupName.Split("_").Select((s, idx) => idx == 1 ? "dbk" : s));
                return rgName;
            }
            throw new Exception("Resource group name not found");
        }
    }
}
