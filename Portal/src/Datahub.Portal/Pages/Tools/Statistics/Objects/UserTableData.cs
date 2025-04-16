using System.Reflection;

namespace Datahub.Portal.Pages.Tools.Statistics.Objects
{
    public class UserTableData : IDHTableData
    {
        public int Id { get; set; }
        public string GraphGuid { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string RegistrationComment { get; set; }
        public DHTableLinkList Workspaces { get; set; } = new();
        public DateTime RegistrationDate { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime FirstLogin { get; set; }

        public static string DataLabel() => "user(s)";

        public static List<DHTablePreset> Presets()
        {
            return
            [
                new DHTablePreset
                {
                    Name = "Default",
                    Description = "Default columns for user statistics",
                    Columns =
                    [
                        typeof(UserTableData).GetProperty(nameof(Name))!,
                        typeof(UserTableData).GetProperty(nameof(Email))!
                    ],
                    Filters = []
                },
                new DHTablePreset
                {
                    Name = "Admin",
                    Description = "Admin columns for user statistics",
                    Columns =
                    [
                        typeof(UserTableData).GetProperty(nameof(GraphGuid)),
                        typeof(UserTableData).GetProperty(nameof(Name))!,
                        typeof(UserTableData).GetProperty(nameof(Email))!,
                        typeof(UserTableData).GetProperty(nameof(LastLogin))!,
                    ],
                    Filters = []
                },
                new DHTablePreset()
                {
                    Name = "Active users",
                    Description = "Users who have logged in within the last 90 days",
                    Filters =
                    [
                        new FilterData
                        {
                            ColumnName = nameof(LastLogin),
                            ColumnType = typeof(DateTime),
                            FilterType = FilterType.GreaterThan,
                            Value = DateTime.UtcNow.AddDays(-90).ToString()
                        }
                    ],
                    Columns =
                    [
                        typeof(UserTableData).GetProperty(nameof(Name))!,
                        typeof(UserTableData).GetProperty(nameof(Email))!,
                        typeof(UserTableData).GetProperty(nameof(LastLogin))!
                    ]
                }
            ];
        }

        public static DHTablePreset DefaultPreset() => Presets()[0];
    }
}