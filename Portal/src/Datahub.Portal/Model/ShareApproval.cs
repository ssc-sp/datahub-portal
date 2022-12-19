namespace Datahub.Portal.Model;

public class ShareApproval
{
    public string Name { get; init; }
    public string ShareUrl { get; init; }    
    public string DocumentUrl { get; init; }
    public bool Read { get; set; }
    public bool Approved { get; set; }
    public object RequestObject { get; init; }
}