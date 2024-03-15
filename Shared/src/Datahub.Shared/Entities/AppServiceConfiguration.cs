using J2N.Collections.ObjectModel;

namespace Datahub.Shared.Entities
{
    public class AppServiceConfiguration
    {
        public string Framework { get; set; } = string.Empty;
        public string GitRepo { get; set; } = string.Empty;
        public string ComposePath { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;

        public AppServiceConfiguration(string framework, string gitRepo,
            string composePath, string id = "", string hostName = "")
        {
            Framework = framework;
            GitRepo = gitRepo;
            ComposePath = composePath;
            Id = id;
            HostName = hostName;
        }
    }

    public static class AppServiceTemplates
    {
        private static readonly AppServiceConfiguration SHINY_CONFIG =
            new(SHINY, "https://github.com/ssc-sp/datahub-infra.git", "dev/docker/shiny-app/");
        private static readonly AppServiceConfiguration CUSTOM_CONFIG =
            new(CUSTOM, string.Empty, string.Empty);

        public const string SHINY = "Shiny";
        public const string CUSTOM = "Docker compose";

        public static readonly List<string> TEMPLATES = [SHINY, CUSTOM];

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