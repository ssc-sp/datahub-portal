namespace ResourceProvisioner.Domain.ValueObjects;

public class PullRequestValueObject
{
    public string WorkspaceAcronym { get; }
    public string Url { get; }
    public int PullRequestId { get; set; }
    public string CreatedById { get; }

    public PullRequestValueObject(string workspaceAcronym, string url, int id, string createdById)
    {
        WorkspaceAcronym = workspaceAcronym;
        Url = url;
        PullRequestId = id;
        CreatedById = createdById;
    }
    
}