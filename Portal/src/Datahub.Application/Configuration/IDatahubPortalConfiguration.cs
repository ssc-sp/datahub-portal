namespace Datahub.Application.Configuration
{
    public interface IDatahubPortalConfiguration
    {
        Achievements Achievements { get; set; }
        AdoOrg AdoOrg { get; set; }
        AdoServiceUser AdoServiceUser { get; set; }
        string AllowedHosts { get; set; }
        string[] AllowedUserEmailDomains { get; set; }
        APITargets APITargets { get; set; }
        ApplicationInsights ApplicationInsights { get; set; }
        AzureAd AzureAd { get; set; }
        string AzureSignalRStickyServerMode { get; set; }
        bool CentralizedProjectSecrets { get; set; }
        CkanConfiguration CkanConfiguration { get; set; }
        ConnectionStrings ConnectionStrings { get; set; }
        CultureSettings CultureSettings { get; set; }
        string DatahubGraphInviteFunctionUrl { get; set; }
        string DatahubGraphUsersStatusFunctionUrl { get; set; }
        string DataHubModules { get; set; }
        DatahubStorageQueue DatahubStorageQueue { get; set; }
        DataProjects DataProjects { get; set; }
        EmailNotification EmailNotification { get; set; }
        GithubConfig Github { get; set; }
        Graph Graph { get; set; }
        Hosting Hosting { get; set; }
        KeyVault KeyVault { get; set; }
        string LandingBgFolder { get; set; }
        int LandingBgImgCount { get; set; }
        Media Media { get; set; }
        string PortalRunAsManagedIdentity { get; set; }
        PreRegistrationDocumentationUrl PreRegistrationDocumentationUrl { get; set; }
        string ProfileUrlSegment { get; set; }
        string ProjectStorageKeySecretName { get; set; }
        string ProjectUrlSegment { get; set; }
        PublicFileSharing PublicFileSharing { get; set; }
        string ResourcePrefix { get; set; }
        ReverseProxy ReverseProxy { get; set; }
        bool ShowLoginPage { get; set; }
        string SupportFormUrl { get; set; }
        TermsAndConditionsUrl TermsAndConditionsUrl { get; set; }
        string Title { get; set; }
    }
}