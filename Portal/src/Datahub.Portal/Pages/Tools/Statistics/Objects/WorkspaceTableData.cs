using System.Reflection;
using Datahub.Portal.Components.Tables;

namespace Datahub.Portal.Pages.Tools.Statistics.Objects
{
    public class WorkspaceTableData : IDHTableData
    {
        public string Title { get; set; }
        public string Acronym { get; set; }
        public string Department { get; set; }
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public string WorkspaceLead { get; set; }
        public string WorkspaceLeadEmail { get; set; }
        public DHTableLinkList Members { get; set; }
        public int MemberCount { get; set; }
        public DHTableLinkList Resources { get; set; }
        public int ResourceCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }

        public static List<DHTablePreset> Presets()
        {
            return new List<DHTablePreset>
            {
                new DHTablePreset
                {
                    Name = "Default",
                    Description = "Default preset for workspace data",
                    Columns = new List<PropertyInfo>
                    {
                        typeof(WorkspaceTableData).GetProperty(nameof(Title))!,
                        typeof(WorkspaceTableData).GetProperty(nameof(Acronym))!,
                        typeof(WorkspaceTableData).GetProperty(nameof(Description))!,
                        typeof(WorkspaceTableData).GetProperty(nameof(CreatedAt))!,
                        typeof(WorkspaceTableData).GetProperty(nameof(UpdatedAt))!
                    },
                    Filters = new List<FilterData>()
                },
                new DHTablePreset
                {
                    Name = "Extended",
                    Description = "Extended preset for workspace data",
                    Columns = new List<PropertyInfo>
                    {
                        typeof(WorkspaceTableData).GetProperty(nameof(Title))!,
                        typeof(WorkspaceTableData).GetProperty(nameof(Acronym))!,
                        typeof(WorkspaceTableData).GetProperty(nameof(Description))!,
                        typeof(WorkspaceTableData).GetProperty(nameof(CreatedAt))!,
                        typeof(WorkspaceTableData).GetProperty(nameof(UpdatedAt))!,
                        typeof(WorkspaceTableData).GetProperty(nameof(Members))!,
                        typeof(WorkspaceTableData).GetProperty(nameof(Resources))!
                    },
                    Filters = new List<FilterData>()
                }
            };
        }

        public static DHTablePreset DefaultPreset() => Presets()[0];

        public static string DataLabel() => "workspace(s)";
    }
}