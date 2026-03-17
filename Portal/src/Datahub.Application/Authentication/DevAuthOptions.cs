namespace Datahub.Application.Authentication;

public class DevAuthOptions
{
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public List<string> Workspaces { get; set; } = [];
}
