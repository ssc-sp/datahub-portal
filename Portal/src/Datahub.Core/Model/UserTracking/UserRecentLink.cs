using Datahub.Core.Components;
using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Model.UserTracking;

public record UserRecentLink
{
    public int Id { get; set; }

    public DatahubLinkType LinkType { get; set; }

    public string PowerBIURL { get; set; }

    public string Name { get; set; }

    public string Variant { get; set; }

    public string DatabricksURL { get; set; }
    public string AzureWebAppUrl { get; set; }

    public string WebFormsURL { get; set; }

    public string DataProject { get; set; }

    public string PBIReportId { get; set; }

    public string PBIWorkspaceId { get; set; }

    public DateTimeOffset AccessedTime { get; set; }

    public string ExternalUrl { get; set; }

    public string ResourceArticleId { get; set; }

    public string ResourceArticleTitle { get; set; }

    public PortalUser User { get; set; }

    public int UserId { get; set; }

    public string ToTitle()
    {
        return LinkType switch
        {
            DatahubLinkType.DataProject => "{0} Workspace",
            DatahubLinkType.Storage => "Storage",
            DatahubLinkType.Databricks => "Databricks",
            DatahubLinkType.ResourceArticle => "Resources",
            DatahubLinkType.AzureWebApp => "Web App",
            _ => string.IsNullOrWhiteSpace(Name) ? "Missing Title" : Name
        };
    }
}