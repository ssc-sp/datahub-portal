using Microsoft.AspNetCore.Components;

namespace Datahub.Core.Data;

public class FormMetadata<T>
{
    public string Header { get; set; }
    public string HeaderSubText { get; set; }
    public string SubHeader { get; set; }
    public string UserId { get; set; }
    public string TableDisclaimer { get; set; }
    public List<T> DataSet { get; set; }
    public Dictionary<string, List<T>> TabbedDataSets { get; set; }

    public IList<Func<T, string>> AccessorFunctions { get; set; }

    public IList<Func<T, RenderFragment>> RenderFunctions { get; set; }
    public IList<RenderFragment> FormButtons { get; set; }
    public IList<string> Headers { get; set; }
    public string GridTemplateColumns { get; set; }

    public IList<string> MarkDownContent { get; set; }

    public IList<string> MarkDownContentFooter { get; set; }

    public IList<Func<T, (string Description, string Url)>> NavigateUrls { get; set; }
    public IList<(Delegate Label, Delegate Choices)> FilterProperties { get; set; }
    public bool AllowSearch { get; set; }

    public bool DisableNew { get; set; }
    public bool IsSubmitEnabled { get; set; }
    public bool IsAddEnabled { get; set; } = true;
    public string SaveButtonText { get; set; } = "Save";

    public bool AllowDelete { get; set; }

    public string TableRoles { get; set; }
    public bool IsLoaded()
    {
        return Header != null && DataSet != null && AccessorFunctions != null && Headers != null && GridTemplateColumns != null;
    }
    public bool IsMudLoaded()
    {
        return Header != null && DataSet != null && AccessorFunctions != null && Headers != null;
    }
}