namespace Datahub.Shared.Entities;

public class TerraformTemplate
{
        public string Name { get; set; }
        public string Version { get; set; }

        public static TerraformTemplate Default => LatestFromName(TerraformVariables.NewProjectTemplate);
        public static TerraformTemplate LatestFromName(string name)
        {
            return new TerraformTemplate()
            {
                Name = name,
                Version = "latest",
            };
        }
}