using System.Text.RegularExpressions;

namespace Datahub.Achievements;

public class DatahubUserTelemetry
{

    private static readonly Regex StorageExplorerRegex = new(@"\/projects\/([a-zA-Z]+)?\/filelist$");
    public string? UserId { get; set; }
    public int NumberOfLogins { get; set; }
    public int NumberOfUsersInvited { get; set; }

    public Dictionary<string, int> VisitedUrls { get; set; } = new();

    // bytes uploaded, 
    // number of invites sent,
    // number of projects created,



    // eanred achiemvents
}