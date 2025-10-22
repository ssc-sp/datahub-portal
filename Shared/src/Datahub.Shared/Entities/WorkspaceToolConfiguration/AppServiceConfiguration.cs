using static System.String;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration
{
    public class AppServiceConfiguration : IWorkspaceToolConfiguration, IWorkspaceToolWithSuffix
    {
        public string Framework { get; set; }
        public string GitRepo { get; set; }
        public bool IsGitRepoPrivate { get; set; }
        public string GitTokenSecretName { get; set; } = "app-service-git-token";
        public string GitToken { get; set; }
        public string ComposePath { get; set; }
        public string Id { get; set; }
        public string HostName { get; set; }
        public string ResourceNameSuffix { get; set; }
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

        public IWorkspaceToolConfiguration Clone()
        {
            return new AppServiceConfiguration()
            {
                Framework = Framework,
                GitRepo = GitRepo,
                IsGitRepoPrivate = IsGitRepoPrivate,
                GitTokenSecretName = GitTokenSecretName,
                GitToken = GitToken,
                ComposePath = ComposePath,
                Id = Id,
                HostName = HostName,
                ResourceNameSuffix = ResourceNameSuffix
            };
        }

        public void WriteToWorkspaceDefinition(WorkspaceDefinition workspaceDefinition)
        {
            workspaceDefinition.AppData.AppServiceConfiguration = this;
        }

        public static IWorkspaceToolConfiguration ReadFromWorkspaceDefinition(WorkspaceDefinition workspaceDefinition)
        {
            return workspaceDefinition.AppData.AppServiceConfiguration ?? new AppServiceConfiguration();
        }

        public static string GetPropertyLabel(string propertyName)
        {
            return propertyName switch
            {
                _ => propertyName
            };
        }

        public string GenerateResourceInputJson()
        {
            return "{}";
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