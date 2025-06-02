namespace Datahub.Portal.Components.Tables
{
    public class FilterData
    {
        public string ColumnName { get; set; }
        public Type ColumnType { get; set; }
        public FilterType FilterType { get; set; }
        public string Value { get; set; }
    }

    public enum FilterType
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        Contains,
        StartsWith,
        EndsWith
    }

    public static class FilterTypeExtensions
    {
        public static string Label(this FilterType filterType)
        {
            return filterType switch
            {
                FilterType.Equals => "Equals",
                FilterType.NotEquals => "Not equals",
                FilterType.GreaterThan => "Greater than",
                FilterType.LessThan => "Less than",
                FilterType.Contains => "Contains",
                FilterType.StartsWith => "Starts with",
                FilterType.EndsWith => "Ends with",
                _ => throw new ArgumentOutOfRangeException(nameof(filterType), filterType, null)
            };
        }

        public static List<FilterType> FilterTypes(this Type type)
        {
            return type switch            
            {
                Type t when t == typeof(string) =>
                [
                    FilterType.Equals,
                    FilterType.NotEquals,
                    FilterType.Contains,
                    FilterType.StartsWith,
                    FilterType.EndsWith
                ],
                Type t when t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) =>
                [
                    FilterType.Equals,
                    FilterType.NotEquals,
                    FilterType.GreaterThan,
                    FilterType.LessThan
                ],
                Type t when t == typeof(DateTime) || t == typeof(DateTimeOffset) =>
                [
                    FilterType.Equals,
                    FilterType.NotEquals,
                    FilterType.GreaterThan,
                    FilterType.LessThan
                ],
                _ => []
            };
        }
    }
}