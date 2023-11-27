using Datahub.Core.Components;
using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.UserTracking;

namespace Datahub.Portal.Pages.Landing;

public partial class RecentLinks
{
    private string GetLinkDescription(UserRecentLink link, Datahub_Project project)
    {
        return link.LinkType switch
        {
            DatahubLinkType.DataProject => project?.ProjectName ?? "-",
            DatahubLinkType.Storage => Localizer["Storage"],
            DatahubLinkType.Databricks => Localizer["Databricks"],
            DatahubLinkType.DataSharingDashboard => Localizer["Data Sharing"],
            DatahubLinkType.ComputeCostEstimator => Localizer["Compute Cost"],
            DatahubLinkType.M365Form => Localizer["Microsoft 365 Data Entry"],
            DatahubLinkType.StorageCostEstimator => Localizer["Storage Cost Estimator"],
            DatahubLinkType.PowerBI => Localizer["PowerBI"],
            DatahubLinkType.PowerBIReport => Localizer["PowerBI Report"],
            DatahubLinkType.PowerBIWorkspace => Localizer["PowerBI Workspace"],
            DatahubLinkType.ResourceArticle => Localizer["Resources"],
            DatahubLinkType.PublishingDashboard => Localizer["Publishing"],
            _ => string.IsNullOrWhiteSpace(link.Name) ? "Power BI" : Localizer[link.Name]
        };
    }
    
    private static string GetIconName(UserRecentLink link, Datahub_Project project)
    {
        return link.LinkType == DatahubLinkType.DataProject 
            ? $"fad fa-{project?.Project_Icon ?? Icon.DEFAULT_PROJECT_ICON}" 
            : GetIcon(link).Name;
    }
    
    private static Icon GetIcon(UserRecentLink link)
    {
        return link.LinkType switch
        {
            DatahubLinkType.PowerBI => Icon.POWERBI,
            DatahubLinkType.Databricks => Icon.DATASETS,
            DatahubLinkType.WebForm => Icon.DATAENTRY,
            DatahubLinkType.DataProject => Icon.PROJECT,
            DatahubLinkType.Storage => Icon.STORAGE,
            DatahubLinkType.FormBuilder => Icon.PROJECT,
            DatahubLinkType.DataSharingDashboard => Icon.PROJECT,
            DatahubLinkType.ResourceArticle => Icon.RESOURCE_ARTICLE,
            DatahubLinkType.PublishingDashboard => Icon.PROJECT,
            _ => Icon.HOME
        };
    }
    
    private string GetDescription(UserRecentLink link, Datahub_Project project)
    {
        var projectName = project?.ProjectName ?? Localizer["Home"];
        return link.LinkType switch
        {
            DatahubLinkType.PowerBI => $"{projectName} >> {Localizer["Power BI"]}",
            DatahubLinkType.Databricks => $"{projectName} >> {Localizer["Databricks"]}",
            DatahubLinkType.WebForm => $"{projectName} >> {Localizer["Data Entry"]}",
            DatahubLinkType.DataProject => $"{projectName} >> {Localizer["Initiative Home"]}",
            DatahubLinkType.Storage => $"{projectName} >> {Localizer["Storage"]}",
            DatahubLinkType.FormBuilder => $"{projectName} >> {Localizer["Form Builder"]}",
            DatahubLinkType.DataSharingDashboard => $"{projectName} >> {Localizer["Data Sharing Dashboard"]}",
            DatahubLinkType.ResourceArticle => link.ResourceArticleTitle,
            DatahubLinkType.PublishingDashboard => $"{projectName} >> {Localizer["Publishing Dashboard"]}",
            _ => "N/A"
        };
    }
}