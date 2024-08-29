using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;

namespace Datahub.Core.Extensions
{
    public static class ProjectResourceGroupExtension
    {
        public static string GetResourceGroupName(this Datahub_Project project)
        {
            var newProjectTemplateType =
                TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate);
            var newProjectResource = project.Resources.FirstOrDefault(r => r.ResourceType == newProjectTemplateType);
            if (newProjectResource is not null && newProjectResource.CreatedAt.HasValue)
            {
                var jsonContent = JsonSerializer.Deserialize<JsonObject>(newProjectResource.JsonContent);
                string rgName = jsonContent["resource_group_name"]!.ToString();
                return rgName;
            }

            throw new Exception("Resource group name not found");
        }

        public static string GetResourceGroupNameFromBlob(this Datahub_Project project)
        {
            var blobStorageTemplateType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
            var blobStorageResource = project.Resources.FirstOrDefault(r => r.ResourceType == blobStorageTemplateType);
            if (blobStorageResource is not null && blobStorageResource.CreatedAt.HasValue)
            {
                var jsonContent = JsonSerializer.Deserialize<JsonObject>(blobStorageResource.JsonContent);
                var rgName = jsonContent["resource_group_name"]!.ToString();
                return rgName;
            }

            throw new Exception("Resource group name not found");
        }

        public static string GetDbkResourceGroupName(this Datahub_Project project, string projectResourceGroupName)
        {
            var dbkResourceTemplateType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
            var dbkResource = project.Resources.FirstOrDefault(r => r.ResourceType == dbkResourceTemplateType);
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