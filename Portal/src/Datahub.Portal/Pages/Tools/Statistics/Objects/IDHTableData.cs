using System.Reflection;

namespace Datahub.Portal.Pages.Tools.Statistics.Objects;

public interface IDHTableData
{
    public static abstract List<DHTablePreset> Presets();
    public static abstract DHTablePreset DefaultPreset();
    public static abstract string DataLabel();
}

public class DHTablePreset
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<PropertyInfo> Columns { get; set; } = null!;
    public List<FilterData> Filters { get; set; } = null!;
}