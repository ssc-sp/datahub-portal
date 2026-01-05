namespace Datahub.Shared.Configuration;

public class AzureDevOpsConfiguration
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SupportIterationName { get; set; } = "POC 2";
    public string AreaPathName { get; set; } = "FSDH Support Team";
    public int? SupportRequestId { get; set; }
    public int? SystemErrorId { get; set; }
    public int? InfrastructureErrorId { get; set; }
    public int? PythonWorkspaceSyncErrorId { get; set; }
    public string OrganizationName { get; set; } = "DataSolutionsDonnees";
    public string ProjectName { get; set; } = "FSDH SSC";
    public string OrganizationUrl => $"https://dev.azure.com/{OrganizationName}";
    public string ListPipelineUrlTemplate { get; set; } = "https://dev.azure.com/{organization}/{project}/_apis/pipelines?api-version=7.1-preview.1";
    public string PostPipelineRunUrlTemplate { get; set; } = "https://dev.azure.com/{organization}/{project}/_apis/pipelines/{pipelineId}/runs?api-version=7.1-preview.1";
    public string AppServiceConfigPipeline { get; set; } = "web-app-configuration";

    public string GetEnvironmentName()
    {
        var envName = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev").ToLower();

        // map developemnt or sandbox to dev
        if (envName.Equals("development") || envName.Equals("sand"))
            return "dev";

        return envName;
    }
}
