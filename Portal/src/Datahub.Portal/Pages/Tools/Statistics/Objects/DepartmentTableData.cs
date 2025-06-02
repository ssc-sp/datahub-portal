using System.Reflection;
using Datahub.Portal.Components.Tables;

namespace Datahub.Portal.Pages.Tools.Statistics.Objects
{
    public class DepartmentTableData : IDHTableData
    {
        public string Name { get; set; }
        public int WorkspaceCount { get; set; }
        public int MemberCount { get; set; }
        public int ResourceCount { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }

        public static List<DHTablePreset> Presets()
        {
            return new List<DHTablePreset>
            {
                new DHTablePreset
                {
                    Name = "Default",
                    Description = "Default preset for department data",
                    Columns =
                    [
                        typeof(DepartmentTableData).GetProperty(nameof(Name))!,
                        typeof(DepartmentTableData).GetProperty(nameof(WorkspaceCount))!,
                        typeof(DepartmentTableData).GetProperty(nameof(MemberCount))!,
                        typeof(DepartmentTableData).GetProperty(nameof(ResourceCount))!,
                        typeof(DepartmentTableData).GetProperty(nameof(TotalBudget))!,
                        typeof(DepartmentTableData).GetProperty(nameof(TotalSpent))!
                    ],
                    Filters = new List<FilterData>()
                }
            };
        }

        public static DHTablePreset DefaultPreset() => Presets()[0];

        public static string DataLabel() => "department(s)";
    }
}
