namespace Datahub.Core.Components.Common;

public class Toast
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public float Progress { get; set; }

    public string Icon { get; set; } = "fa-file-alt";
}