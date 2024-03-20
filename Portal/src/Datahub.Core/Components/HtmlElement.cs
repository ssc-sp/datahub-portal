using Microsoft.AspNetCore.Components;

namespace Datahub.Core.Components;

public class HtmlElement : ComponentBase
{
    [Parameter]
    public RenderFragment ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> InputAttributes { get; set; }
    public Dictionary<string, object> InputAttributesWithoutClass { get; set; }
    protected string InputClass => InputAttributes != null && InputAttributes.ContainsKey("class") ? InputAttributes["class"] as string : string.Empty;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        InputAttributesWithoutClass = InputAttributes?
            .Keys
            .Where(k => k != "class")
            .ToDictionary(_ => _, _ => InputAttributes[_]);
    }
}