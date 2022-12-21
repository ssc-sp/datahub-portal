namespace Datahub.Core.Data;

public class FilterBarOptions
{
    public bool ShowCheckBox { get; set; }
    public bool ShowTextSearch { get; set; }
    public string SearchPlaceHolderText { get; set; } = string.Empty;
    public string CheckBoxText { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
}