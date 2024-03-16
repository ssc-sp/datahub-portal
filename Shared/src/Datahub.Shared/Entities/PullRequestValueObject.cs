namespace Datahub.Shared.Entities;

public class PullRequestValueObject
{
    public string WorkspaceAcronym { get; }
    public string Url { get; }
    public int PullRequestId { get; set; }

    public PullRequestValueObject(string workspaceAcronym, string url, int id)
    {
        WorkspaceAcronym = workspaceAcronym;
        Url = url;
        PullRequestId = id;
    }

}