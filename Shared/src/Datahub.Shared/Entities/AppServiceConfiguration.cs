namespace Datahub.Shared.Entities
{
    public class AppServiceConfiguration
    {
        public string Framework { get; set; } = string.Empty;
        public string GitRepo { get; set; } = string.Empty;
        public bool IsGitRepoPrivate { get; set; } = false;
        public string GitTokenSecretName { get; set; } = "app-service-git-token";
        public string GitToken { get; set; } = string.Empty;
        public string ComposePath { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;

        public AppServiceConfiguration(string framework, string gitRepo,
            string composePath, string id = "", string hostName = "", bool visibility = false, string gitTokenSecretName = "app-service-git-token")
        {
            Framework = framework;
            GitRepo = gitRepo;
            ComposePath = composePath;
            Id = id;
            HostName = hostName;
            IsGitRepoPrivate = visibility;
            GitTokenSecretName = gitTokenSecretName;
        }
    }

    public static class AppServiceTemplates
    {
        private static AppServiceConfiguration SHINY_CONFIG =
            new(SHINY, "https://github.com/ssc-sp/datahub-infra.git", "dev/docker/shiny-app/");
        private static AppServiceConfiguration CUSTOM_CONFIG =
            new(CUSTOM, "", "");

        public const string SHINY = "Shiny";
        public const string CUSTOM = "Docker compose";

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