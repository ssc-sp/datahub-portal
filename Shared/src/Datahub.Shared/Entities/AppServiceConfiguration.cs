namespace Datahub.Shared.Entities
{
    public class AppServiceConfiguration
    {
        public string AppServiceFramework { get; set; } = string.Empty;
        public string AppServiceGitRepo { get; set; } = string.Empty;
        public string AppServiceComposePath { get; set; } = string.Empty;
        public string AppServiceId { get; set; } = string.Empty;
        public string AppServiceHostName { get; set; } = string.Empty;
        public string AppServiceRg { get; set; } = string.Empty;

        public AppServiceConfiguration(string appServiceFramework, string appServiceGitRepo,
            string appServiceComposePath, string appServiceId = "", string appServiceHostName = "", string appServiceRg = "")
        {
            AppServiceFramework = appServiceFramework;
            AppServiceGitRepo = appServiceGitRepo;
            AppServiceComposePath = appServiceComposePath;
            AppServiceId = appServiceId;
            AppServiceHostName = appServiceHostName;
            AppServiceRg = appServiceRg;
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