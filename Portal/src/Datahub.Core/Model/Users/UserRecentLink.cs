using Datahub.Core.Components;

namespace Datahub.Core.Model.Users
{
    /// <summary>
    /// Represents a link recently accessed by a user, capturing related URL and metadata information.
    /// </summary>
    public record UserRecentLink
    {
        /// <summary>
        /// Gets or sets the unique identifier of this recent link record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the link.
        /// </summary>
        public DatahubLinkType LinkType { get; set; }

        /// <summary>
        /// Gets or sets the Power BI URL of the link.
        /// </summary>
        public string? PowerBIURL { get; set; }

        /// <summary>
        /// Gets or sets the name or display title for this link.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the link variant to distinguish different types of items.
        /// </summary>
        public string? Variant { get; set; }

        /// <summary>
        /// Gets or sets the Databricks URL of the link.
        /// </summary>
        public string? DatabricksURL { get; set; }

        /// <summary>
        /// Gets or sets the Azure web application URL of the link.
        /// </summary>
        public string? AzureWebAppUrl { get; set; }

        /// <summary>
        /// Gets or sets the web forms URL of the link.
        /// </summary>
        public string? WebFormsURL { get; set; }

        /// <summary>
        /// Gets or sets the data project title or identifier.
        /// </summary>
        public string? DataProject { get; set; }

        /// <summary>
        /// Gets or sets the Power BI report ID if applicable.
        /// </summary>
        public string? PBIReportId { get; set; }

        /// <summary>
        /// Gets or sets the Power BI workspace ID if applicable.
        /// </summary>
        public string? PBIWorkspaceId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this link was last accessed.
        /// </summary>
        public DateTimeOffset AccessedTime { get; set; }

        /// <summary>
        /// Gets or sets an external URL if the link points outside the application.
        /// </summary>
        public string? ExternalUrl { get; set; }

        /// <summary>
        /// Gets or sets the ID of a resource article.
        /// </summary>
        public string? ResourceArticleId { get; set; }

        /// <summary>
        /// Gets or sets the title of a resource article.
        /// </summary>
        public string? ResourceArticleTitle { get; set; }

        /// <summary>
        /// Gets or sets the related user for this link record.
        /// </summary>
        public PortalUser? User { get; set; }

        /// <summary>
        /// Gets or sets the foreign key for the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Returns a title string for this link based on its link type or name.
        /// </summary>
        /// <returns>A formatted title string.</returns>
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
}