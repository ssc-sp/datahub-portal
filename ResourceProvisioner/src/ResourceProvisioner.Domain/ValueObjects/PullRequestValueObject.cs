namespace ResourceProvisioner.Domain.ValueObjects;

public class PullRequestValueObject
{
    public string WorkspaceAcronym { get; }
    public string Url { get; }

    public PullRequestValueObject(string workspaceAcronym, string url)
    {
        WorkspaceAcronym = workspaceAcronym;
        Url = url;
    }
    
}