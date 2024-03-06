namespace Datahub.Core.Data.ExternalSearch.OpenData;

public class OpenDataOrganization
{
    public Guid Id { get; set; }
    public Guid RevisionId { get; set; }
    public string Title { get; set; }
    public string Name { get; set; }
    public string ApprovalStatus { get; set; }
    public DateTime Created { get; set; }
    public string Description { get; set; }
    public string State { get; set; }
    public string ImageUrl { get; set; }
    public bool IsOrganization { get; set; }
    public string Type { get; set; }
}