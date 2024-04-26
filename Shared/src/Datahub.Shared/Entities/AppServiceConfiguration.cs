using static System.String;

namespace Datahub.Shared.Entities
{
    public class AppServiceConfiguration
    {
        public string Framework { get; set; }
        public string GitRepo { get; set; }
        public bool IsGitRepoPrivate { get; set; }
        public string GitTokenSecretName { get; set; } = "app-service-git-token";
        public string GitToken { get; set; }
        public string ComposePath { get; set; }
        public string Id { get; set; }
        public string HostName { get; set; }

        public AppServiceConfiguration() { }

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
        //private static readonly AppServiceConfiguration SHINY_CONFIG =
        //    new(SHINY, "https://github.com/ssc-sp/datahub-infra.git", "dev/docker/shiny-app/");
        private static readonly AppServiceConfiguration CUSTOM_CONFIG =
            new(CUSTOM, Empty, Empty);

        // public const string SHINY = "Shiny";
        public const string CUSTOM = "Docker compose";

        public static readonly List<string> TEMPLATES = [CUSTOM];

        public static AppServiceConfiguration GetTemplateConfiguration(string template)
        {
            return template switch
            {
                //SHINY => SHINY_CONFIG,
                CUSTOM => CUSTOM_CONFIG,
                _ => null
            };
        }
    }
}