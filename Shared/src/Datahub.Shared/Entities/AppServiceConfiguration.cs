namespace Datahub.Shared.Entities
{
    public class AppServiceConfiguration
    {
        public string Framework { get; set; } = string.Empty;
        public string GitRepo { get; set; } = string.Empty;
        public string ComposePath { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string ResourceGroupName { get; set; } = string.Empty;

        public AppServiceConfiguration(string framework, string gitRepo,
            string composePath, string id = "", string hostName = "", string resourceGroupName = "")
        {
            Framework = framework;
            GitRepo = gitRepo;
            ComposePath = composePath;
            Id = id;
            HostName = hostName;
            ResourceGroupName = resourceGroupName;
        }
    }

    public static class AppServiceTemplates
    {
        private static AppServiceConfiguration SHINY_CONFIG =
            new("shiny", "https://github.com/ssc-sp/datahub-infra.git", "dev/docker/shiny-app/");
        private static AppServiceConfiguration CUSTOM_CONFIG =
            new("", "", "");

        public const string SHINY = "Shiny";
        public const string CUSTOM = "Custom";

        public static List<string> TEMPLATES = [SHINY, CUSTOM];

        public static AppServiceConfiguration GetTemplateConfiguration(string template)
        {
            return template switch
            {
                SHINY => SHINY_CONFIG,
                CUSTOM => CUSTOM_CONFIG,
                _ => null
            };
        }
    }
}